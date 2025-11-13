using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AirWatch.Api.Services.Interfaces;
using AirWatch.Api.DTOs.External;

namespace AirWatch.Api.Services
{
    public class OpenWeatherMapService : IOpenWeatherMapService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OpenWeatherMapService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly TimeSpan _cacheDuration;

        public OpenWeatherMapService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<OpenWeatherMapService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _apiKey = configuration["OpenWeatherMap:ApiKey"] ?? throw new Exception("OpenWeatherMap API key is not configured.");
            _baseUrl = "https://api.openweathermap.org/data/2.5";
            _cacheDuration = TimeSpan.FromMinutes(15);
        }

        public async Task<AirPollutionResponse> GetCurrentPollutionAsync(decimal latitude, decimal longitude)
        {
            var cacheKey = $"pollution_current_{latitude}_{longitude}";

            if (_cache.TryGetValue<AirPollutionResponse>(cacheKey, out var cachedData))
            {
                return cachedData ?? throw new Exception("Cached data is null");
            }

            try
            {
                var url = $"{_baseUrl}/air_pollution?lat={latitude}&lon={longitude}&appid={_apiKey}";
                var response = await _httpClient.GetFromJsonAsync<AirPollutionResponse>(url);

                if (response == null)
                {
                    throw new Exception("Null response from OpenWeatherMap API");
                }

                _cache.Set(cacheKey, response, _cacheDuration);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current pollution data for coordinates {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }

        public async Task<AirPollutionHistoryResponse> GetPollutionHistoryAsync(decimal latitude, decimal longitude, int hours)
        {
            var cacheKey = $"pollution_history_{latitude}_{longitude}_{hours}";

            if (_cache.TryGetValue<AirPollutionHistoryResponse>(cacheKey, out var cachedData))
            {
                return cachedData ?? throw new Exception("Cached data is null");
            }

            try
            {
                var endTime = DateTimeOffset.UtcNow;
                var startTime = endTime.AddHours(-hours);

                var url = $"{_baseUrl}/air_pollution/history?" +
                         $"lat={latitude}&lon={longitude}" +
                         $"&start={startTime.ToUnixTimeSeconds()}" +
                         $"&end={endTime.ToUnixTimeSeconds()}" +
                         $"&appid={_apiKey}";

                var response = await _httpClient.GetFromJsonAsync<AirPollutionHistoryResponse>(url);

                if (response == null)
                {
                    throw new Exception("Null response from OpenWeatherMap API");
                }

                _cache.Set(cacheKey, response, _cacheDuration);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pollution history for coordinates {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }
    }
}