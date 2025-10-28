using System;

namespace AirWatch.Api.DTOs.Pollution;

/// <summary>
/// Response for current pollution data.
/// Endpoint: GET /api/pollution/current?lat={lat}&lon={lon}
/// </summary>
public class PollutionCurrentResponse
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int AQI { get; set; }
    public PollutantsDto Pollutants { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public TimeSpan DataAge { get; set; }
}
