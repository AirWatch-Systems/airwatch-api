using System;

namespace AirWatch.Api.Models.Entities
{
    /// <summary>
    /// Represents a user's search history for locations
    /// </summary>
    public class SearchHistory
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public string LocationName { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime SearchedAt { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
