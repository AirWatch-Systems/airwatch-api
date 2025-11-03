using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;

namespace AirWatch.Api.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Feedback feedback, CancellationToken ct = default);

        /// <summary>
        /// Returns feedbacks within the given radius (km) and time window (hours) around a coordinate.
        /// Uses a bounding box approximation for performance.
        /// </summary>
        Task<List<Feedback>> GetRecentByLocationAsync(decimal lat, decimal lon, double radiusKm, int hours, CancellationToken ct = default);

        /// <summary>
        /// Returns feedbacks by user (paged, ordered by CreatedAt desc).
        /// </summary>
        Task<List<Feedback>> GetByUserAsync(Guid userId, int skip = 0, int take = 50, CancellationToken ct = default);

        /// <summary>
        /// Gets the most recent feedback by user within a radius from the given coordinates.
        /// </summary>
        Task<Feedback?> GetLatestByUserInRegionAsync(Guid userId, decimal lat, decimal lon, double radiusKm, CancellationToken ct = default);
    }
}