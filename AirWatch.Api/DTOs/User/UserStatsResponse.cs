using System;

namespace AirWatch.Api.DTOs.User
{
    public class UserStatsResponse
    {
        public int TotalSearches { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public int DaysSinceRegistration { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}