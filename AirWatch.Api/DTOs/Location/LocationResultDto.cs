using System;

namespace AirWatch.Api.DTOs.Location;

/// <summary>
/// Single result from location search/autocomplete.
/// </summary>
public class LocationResultDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PlaceId { get; set; } = string.Empty;
}
