using System;
using System.Collections.Generic;

namespace AirWatch.Api.DTOs.Location;

/// <summary>
/// Response for GET /api/locations/search
/// </summary>
public class LocationSearchResponse
{
    public string Query { get; set; } = string.Empty;
    public List<LocationResultDto> Results { get; set; } = new();
    public int Total { get; set; }
}
