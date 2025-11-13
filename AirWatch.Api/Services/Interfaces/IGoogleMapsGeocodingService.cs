using AirWatch.Api.DTOs.Location;

namespace AirWatch.Api.Services.Interfaces
{
    public interface IGoogleMapsGeocodingService
    {
        Task<List<LocationResultDto>> SearchAsync(string query, CancellationToken ct);
    }
}