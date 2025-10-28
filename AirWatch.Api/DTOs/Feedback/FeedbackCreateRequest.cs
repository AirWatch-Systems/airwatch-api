using System;
using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Feedback;

/// <summary>
/// Request body for creating a new feedback.
/// Endpoint: POST /api/feedbacks
/// </summary>
public sealed class FeedbackCreateRequest
{
    [Required]
    public decimal Lat { get; set; }

    [Required]
    public decimal Lon { get; set; }

    /// <summary>
    /// Rating from 1 (worst) to 5 (best).
    /// UI labels (pt-BR): Boa, Normal, Ruim, Muito Ruim, Péssima (mapped by client).
    /// </summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Optional comment, max 500 chars.
    /// </summary>
    [StringLength(500)]
    public string? Comment { get; set; }
}
