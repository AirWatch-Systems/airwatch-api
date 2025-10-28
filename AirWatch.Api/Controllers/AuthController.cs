using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Auth;
using AirWatch.Api.DTOs.Common;
using AirWatch.Api.Models.Entities;
using AirWatch.Api.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthController> _logger;

        private const string CachePrefix = "auth:session:";
        private static readonly TimeSpan TwoFaTtl = TimeSpan.FromMinutes(5);
        private const int BcryptWorkFactor = 12; // A good default work factor for BCrypt

        public AuthController(
            IUserRepository users,
            IConfiguration config,
            IMemoryCache cache,
            ILogger<AuthController> logger)
        {
            _users = users;
            _config = config;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest body, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Additional password confirmation check (redundant with [Compare] attribute, but explicit)
            if (body.Password != body.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(body.ConfirmPassword), "Passwords do not match");
                return ValidationProblem(ModelState);
            }

            var email = body.Email.Trim().ToLowerInvariant();
            var emailExists = await _users.EmailExistsAsync(email, ct);
            if (emailExists)
            {
                _logger.LogWarning("Registration attempt failed for an already existing email: {Email}", email);
                return Conflict(new ErrorResponse("Email already in use"));
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = body.Name.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(body.Password, BcryptWorkFactor),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user, ct);
            _logger.LogInformation("New user registered with ID {UserId}", user.Id);

            return Ok(new RegisterResponse(user.Id, "User registered successfully"));
        }

        /// <summary>
        /// Authenticates a user and initiates the two-factor authentication process.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest body, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var email = body.Email.Trim().ToLowerInvariant();
            var user = await _users.GetByEmailAsync(email, ct);

            if (user is null || !BCrypt.Net.BCrypt.EnhancedVerify(body.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", email);
                return Unauthorized(new ErrorResponse("Invalid credentials"));
            }

            var sessionId = $"sess_{Guid.NewGuid():N}";
            var code = GenerateNumericCode(6);
            var cacheKey = $"{CachePrefix}{sessionId}";

            _cache.Set(cacheKey, new Pending2Fa(user.Id, code), TwoFaTtl);

            // SECURITY: In a production environment, this code must be sent via a secure out-of-band channel
            // (e.g., SMS, email, or an authenticator app). Logging it is insecure and for development only.
            _logger.LogInformation("2FA code for session {SessionId}: {Code}. User: {UserId}", sessionId, code, user.Id);

            return Ok(new LoginResponse(true, sessionId));
        }

        /// <summary>
        /// Verifies the two-factor authentication code and issues a JWT if valid.
        /// </summary>
        [HttpPost("verify-2fa")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Verify2FaResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<ActionResult<Verify2FaResponse>> Verify2Fa([FromBody] Verify2FaRequest body, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var cacheKey = $"{CachePrefix}{body.SessionId}";
            if (!_cache.TryGetValue<Pending2Fa>(cacheKey, out var pending) || pending == null)
            {
                _logger.LogWarning("2FA verification failed due to invalid or expired session: {SessionId}", body.SessionId);
                return Unauthorized(new ErrorResponse("Invalid or expired session"));
            }

            if (!string.Equals(pending.Code, body.Token, StringComparison.Ordinal))
            {
                _logger.LogWarning("Invalid 2FA token provided for session: {SessionId}, User: {UserId}", body.SessionId, pending.UserId);
                return Unauthorized(new ErrorResponse("Invalid 2FA token"));
            }

            _cache.Remove(cacheKey);
            _logger.LogInformation("2FA verification successful for user: {UserId}", pending.UserId);

            (string token, int expiresIn) = await GenerateJwtAsync(pending.UserId);

            return Ok(new Verify2FaResponse(token, expiresIn));
        }

        private Task<(string token, int expiresIn)> GenerateJwtAsync(Guid userId)
        {
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? _config["Jwt:Secret"];

            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogCritical("JWT secret is not configured. Application cannot issue authentication tokens.");
                throw new InvalidOperationException("JWT secret key is not configured.");
            }
            if (secret.Length < 32)
            {
                _logger.LogCritical("JWT secret is too short. It must be at least 32 characters long for HMAC-SHA256 security.");
                throw new InvalidOperationException("JWT secret key is too short. It must be at least 32 characters long.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow.AddSeconds(-30),
                expires: expires,
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds;

            return Task.FromResult((jwt, expiresIn));
        }

        private static string GenerateNumericCode(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var code = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                code.Append(bytes[i] % 10);
            }
            return code.ToString();
        }
    }
}
