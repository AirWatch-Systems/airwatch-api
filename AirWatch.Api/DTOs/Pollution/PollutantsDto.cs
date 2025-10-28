using System;

namespace AirWatch.Api.DTOs.Pollution;

/// <summary>
/// Pollutant readings for a single timestamp or the "current" reading.
/// Units:
/// - PM2.5 / PM10: μg/m³
/// - CO/NO2/SO2/O3: μg/m³ (normalized as needed by backend)
/// </summary>
public class PollutantsDto
{
    public decimal PM25 { get; set; }
    public decimal PM10 { get; set; }
    public decimal CO { get; set; }
    public decimal NO2 { get; set; }
    public decimal SO2 { get; set; }
    public decimal O3 { get; set; }
}
