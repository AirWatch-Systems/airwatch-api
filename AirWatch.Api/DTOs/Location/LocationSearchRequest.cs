using System;
using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Location;

/// <summary>
/// Request model for text-based location search.
/// Endpoint: GET /api/locations/search?query={text}
/// </summary>
public sealed class LocationSearchRequest
{
    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string Query { get; set; } = default!;
}
