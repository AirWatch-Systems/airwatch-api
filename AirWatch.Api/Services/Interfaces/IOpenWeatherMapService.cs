using AirWatch.Api.DTOs.External;

namespace AirWatch.Api.Services.Interfaces
{
    public interface IOpenWeatherMapService
    {
        Task<AirPollutionResponse> GetCurrentPollutionAsync(decimal latitude, decimal longitude);
        Task<AirPollutionHistoryResponse> GetPollutionHistoryAsync(decimal latitude, decimal longitude, int hours);
    }
}