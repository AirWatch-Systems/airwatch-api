using System;

namespace AirWatch.Api.Models.Entities
{
    /// <summary>
    /// Represents user feedback about air quality at a specific location
    /// </summary>
    public class Feedback
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Rating { get; set; }         // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
