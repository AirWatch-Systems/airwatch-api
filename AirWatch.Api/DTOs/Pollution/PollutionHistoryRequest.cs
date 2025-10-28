using System;
using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Pollution;

/// <summary>
/// Request model for pollution history query.
/// Endpoint: GET /api/pollution/history?lat={lat}&lon={lon}&hours={hours}
/// Note: For GET endpoints, this model can represent the validated query string.
/// </summary>
public sealed class PollutionHistoryRequest
{
    [Required]
    public decimal Lat { get; set; }

    [Required]
    public decimal Lon { get; set; }

    [Range(1, 168, ErrorMessage = "Hours must be between 1 and 168.")]
    public int Hours { get; set; } = 24;
}
