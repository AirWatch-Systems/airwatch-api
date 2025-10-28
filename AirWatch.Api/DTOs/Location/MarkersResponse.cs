using System;
using System.Collections.Generic;

namespace AirWatch.Api.DTOs.Location;

/// <summary>
/// Response for GET /api/locations/markers
/// </summary>
public class MarkersResponse
{
    public List<RegionMarkerDto> Markers { get; set; } = new();
    public int Total { get; set; }
    public object? Bounds { get; set; }
}
