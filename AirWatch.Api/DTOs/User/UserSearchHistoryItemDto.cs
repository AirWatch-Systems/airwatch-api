using System;

namespace AirWatch.Api.DTOs.User;

/// <summary>
/// Item representing a user's search history entry.
/// </summary>
public class UserSearchHistoryItemDto
{
    public Guid Id { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime SearchedAt { get; set; }
}
