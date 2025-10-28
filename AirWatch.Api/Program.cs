using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using AirWatch.Api;
using AirWatch.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Serilog configuration
// ----------------------------
var logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "Logs/log-.txt";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// ----------------------------
// Configuration and variables
// ----------------------------
var configuration = builder.Configuration;

// Database connection string (env var preferred)
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ??
    configuration.GetConnectionString("DefaultConnection");

// JWT secret (env var preferred)
var jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET") ??
    configuration["Jwt:Secret"];

// CORS allowed origins (comma-separated)
var allowedOrigins =
    Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ??
    configuration["Cors:AllowedOrigins"];

// API Keys (consumed by services, not directly here)
var openWeatherKey =
    Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY") ??
    configuration["OpenWeatherMap:ApiKey"];

var googleMapsKey =
    Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY") ??
    configuration["Google:MapsApiKey"];

// Firebase credentials (JSON string)
var firebaseCredentialsJson =
    Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS") ??
    configuration["Firebase:Credentials"];

// ----------------------------
// Services registration
// ----------------------------
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Default System.Text.Json options are fine; keep camelCase
        opts.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AirWatch API",
        Version = "v1",
        Description = "REST API for the Air Quality Monitoring System (AirWatch)"
    });
    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT as: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// EF Core - SQL Server
builder.Services.AddDbContext<AirWatchDbContext>(options =>
{
    var cs = !string.IsNullOrWhiteSpace(connectionString)
        ? connectionString
        : "Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;";
    options.UseSqlServer(cs);
});

// In-memory cache for short-lived data (e.g., pollution cache lookups)
builder.Services.AddMemoryCache();

// Repositories (DI)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
builder.Services.AddScoped<IPollutionCacheRepository, PollutionCacheRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (!string.IsNullOrWhiteSpace(allowedOrigins))
        {
            var origins = allowedOrigins
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            // Development-friendly: open CORS if no origin configured
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// Authentication - JWT
var signingKeyBytes = Encoding.UTF8.GetBytes(
    string.IsNullOrWhiteSpace(jwtSecret)
        ? "CHANGE_ME_DEV_SECRET_32CHARS_MINIMUM_123456" // dev fallback
        : jwtSecret);

var signingKey = new SymmetricSecurityKey(signingKeyBytes);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Desabilitado para desenvolvimento
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// HttpClient for external APIs
builder.Services.AddHttpClient("OpenWeatherMap", c =>
{
    c.BaseAddress = new Uri("https://api.openweathermap.org/");
    // The key should be appended by services when calling endpoints
});

builder.Services.AddHttpClient("GoogleMaps", c =>
{
    c.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/");
    // The key should be appended by services when calling endpoints
});

// ----------------------------
// Build application
// ----------------------------
var app = builder.Build();

// ----------------------------
// Firebase initialization (comentado para desenvolvimento)
// ----------------------------
try
{
    if (FirebaseApp.DefaultInstance == null)
    {
        if (!string.IsNullOrWhiteSpace(firebaseCredentialsJson))
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(firebaseCredentialsJson)
            });
            Log.Information("Firebase initialized using provided JSON credentials.");
        }
        else
        {
            // Try ADC (Application Default Credentials)
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault()
            });
            Log.Information("Firebase initialized using application default credentials.");
        }
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "Firebase initialization skipped or failed. 2FA/Push may not work until configured.");
}

Log.Information("Firebase initialization skipped for development. 2FA/Push features disabled.");

// ----------------------------
// Middleware pipeline
// ----------------------------

// Global exception handler (simple)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal Server Error",
            traceId = context.TraceIdentifier
        });
    }
});

// Serilog request logging
app.UseSerilogRequestLogging();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AirWatch API v1");
    c.RoutePrefix = "swagger";
});

// HTTPS redirection (comentado para desenvolvimento)
// app.UseHttpsRedirection();

// CORS
app.UseCors("Default");

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Simple health check
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("HealthCheck");

app.Run();
