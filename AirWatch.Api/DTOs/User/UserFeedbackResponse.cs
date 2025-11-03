using System.Collections.Generic;

namespace AirWatch.Api.DTOs.User
{
    public class UserFeedbackResponse
    {
        public List<UserFeedbackItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}