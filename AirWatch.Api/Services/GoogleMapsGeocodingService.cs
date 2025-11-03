using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Location;
using Microsoft.Extensions.Configuration;

namespace AirWatch.Api.Services
{
    public interface IGoogleMapsGeocodingService
    {
        Task<List<LocationResultDto>> SearchAsync(string query, CancellationToken ct);
    }

    public class GoogleMapsGeocodingService : IGoogleMapsGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleMapsGeocodingService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<List<LocationResultDto>> SearchAsync(string query, CancellationToken ct)
        {
            var url = $"geocode/json?address={Uri.EscapeDataString(query)}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonDocument.Parse(json);
            
            var locations = new List<LocationResultDto>();

            if (result.RootElement.TryGetProperty("results", out var results))
            {
                foreach (var item in results.EnumerateArray())
                {
                    var geometry = item.GetProperty("geometry").GetProperty("location");
                    locations.Add(new LocationResultDto
                    {
                        Name = item.GetProperty("formatted_address").GetString() ?? string.Empty,
                        Latitude = (decimal)geometry.GetProperty("lat").GetDouble(),
                        Longitude = (decimal)geometry.GetProperty("lng").GetDouble(),
                        Address = item.GetProperty("formatted_address").GetString() ?? string.Empty,
                        PlaceId = item.GetProperty("place_id").GetString() ?? string.Empty
                    });
                }
            }
            return locations;
        }
    }
}
