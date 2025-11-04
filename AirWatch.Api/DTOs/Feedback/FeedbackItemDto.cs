using System;

namespace AirWatch.Api.DTOs.Feedback;

/// <summary>
/// Feedback item for list/detail responses.
/// </summary>
public class FeedbackItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
