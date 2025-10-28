using System;

namespace AirWatch.Api.DTOs.Pollution;

/// <summary>
/// Data point of pollution time series.
/// </summary>
public class PollutionHistoryPointDto
{
    public DateTime Timestamp { get; set; }
    public int AQI { get; set; }
    public decimal PM25 { get; set; }
    public decimal PM10 { get; set; }
    public decimal CO { get; set; }
    public decimal NO2 { get; set; }
    public decimal SO2 { get; set; }
    public decimal O3 { get; set; }
}
