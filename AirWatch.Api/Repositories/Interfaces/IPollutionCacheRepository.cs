using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;

namespace AirWatch.Api.Repositories
{
    public interface IPollutionCacheRepository
    {
        Task AddAsync(PollutionCache cache, CancellationToken ct = default);

        /// <summary>
        /// Gets the latest cached pollution near the target coordinate, limited by maxAgeMinutes and a small radius (km).
        /// The geo match uses a bounding box approximation for performance.
        /// </summary>
        Task<PollutionCache?> GetLatestForLocationAsync(decimal lat, decimal lon, int maxAgeMinutes = 60, double radiusKm = 2.0, CancellationToken ct = default);

        /// <summary>
        /// Gets pollution history for a coordinate within the last N hours (uses exact lat/lon match by default).
        /// </summary>
        Task<List<PollutionCache>> GetHistoryAsync(decimal lat, decimal lon, int hours, CancellationToken ct = default);
    }
}