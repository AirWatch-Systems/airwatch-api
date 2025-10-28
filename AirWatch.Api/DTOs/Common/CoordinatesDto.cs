using System;

namespace AirWatch.Api.DTOs.Common;

/// <summary>
/// Latitude/Longitude coordinates.
/// </summary>
public readonly record struct CoordinatesDto(decimal Lat, decimal Lon);
