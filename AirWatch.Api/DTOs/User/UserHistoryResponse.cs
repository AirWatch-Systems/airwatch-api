using System;
using System.Collections.Generic;

namespace AirWatch.Api.DTOs.User;

/// <summary>
/// Response for GET /api/user/history
/// </summary>
public class UserHistoryResponse
{
    public List<UserSearchHistoryItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
