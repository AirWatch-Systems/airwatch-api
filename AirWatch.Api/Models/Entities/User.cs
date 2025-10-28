using System;
using System.Collections.Generic;

namespace AirWatch.Api.Models.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}
