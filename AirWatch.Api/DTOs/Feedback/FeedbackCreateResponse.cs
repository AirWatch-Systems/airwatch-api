using System;

namespace AirWatch.Api.DTOs.Feedback;

/// <summary>
/// Response for POST /api/feedbacks
/// </summary>
public readonly record struct FeedbackCreateResponse(Guid FeedbackId, string Message);
