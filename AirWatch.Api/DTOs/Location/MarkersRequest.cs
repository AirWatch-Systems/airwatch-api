using System;
using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Location;

/// <summary>
/// Request model for fetching markers within map bounds.
/// Endpoint: GET /api/locations/markers?north={north}&south={south}&east={east}&west={west}
/// </summary>
public sealed class MarkersRequest
{
    [Required]
    public decimal North { get; set; }

    [Required]
    public decimal South { get; set; }

    [Required]
    public decimal East { get; set; }

    [Required]
    public decimal West { get; set; }
}
