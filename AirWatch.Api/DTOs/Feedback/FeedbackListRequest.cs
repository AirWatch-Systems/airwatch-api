using System;
using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Feedback;

/// <summary>
/// Request model for listing feedbacks by location/time window.
/// Endpoint: GET /api/feedbacks?lat={lat}&lon={lon}&radius={radius}&hours={hours}
/// </summary>
public sealed class FeedbackListRequest
{
    [Required]
    public decimal Lat { get; set; }

    [Required]
    public decimal Lon { get; set; }

    /// <summary>
    /// Radius in kilometers.
    /// </summary>
    [Range(0.1, 100.0)]
    public double Radius { get; set; } = 5.0;

    /// <summary>
    /// Lookback window in hours.
    /// </summary>
    [Range(1, 168)]
    public int Hours { get; set; } = 12;
}
