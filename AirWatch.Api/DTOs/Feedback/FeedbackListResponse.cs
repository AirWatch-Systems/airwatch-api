using System;
using System.Collections.Generic;

namespace AirWatch.Api.DTOs.Feedback;

/// <summary>
/// Response for GET /api/feedbacks
/// </summary>
public class FeedbackListResponse
{
    public List<FeedbackItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
