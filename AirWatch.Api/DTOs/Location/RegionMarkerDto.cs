using System;

namespace AirWatch.Api.DTOs.Location;

/// <summary>
/// Marker for pollution data visualization on maps.
/// </summary>
public class RegionMarkerDto
{
    public Guid Id { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int AQI { get; set; }
    public decimal PM25 { get; set; }
    public decimal PM10 { get; set; }
    public DateTime LastUpdated { get; set; }
}