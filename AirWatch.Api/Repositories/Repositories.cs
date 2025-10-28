using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api; // AirWatchDbContext
using AirWatch.Api.Models.Entities; // Entities
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Api.Repositories
{
    // Interfaces

    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
    }

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
    }

    public interface ISearchHistoryRepository
    {
        Task AddAsync(SearchHistory entry, CancellationToken ct = default);
        Task<List<SearchHistory>> GetByUserAsync(Guid userId, int skip = 0, int take = 100, CancellationToken ct = default);
    }

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

    // Implementations

    internal sealed class UserRepository : IUserRepository
    {
        private readonly AirWatchDbContext _db;

        public UserRepository(AirWatchDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        {
            return _db.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync(ct);
        }
    }

    internal sealed class FeedbackRepository : IFeedbackRepository
    {
        private readonly AirWatchDbContext _db;

        public FeedbackRepository(AirWatchDbContext db)
        {
            _db = db;
        }

        public Task<Feedback?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);
        }

        public async Task AddAsync(Feedback feedback, CancellationToken ct = default)
        {
            if (feedback.Id == Guid.Empty)
                feedback.Id = Guid.NewGuid();

            if (feedback.CreatedAt == default)
                feedback.CreatedAt = DateTime.UtcNow;

            await _db.Feedbacks.AddAsync(feedback, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<Feedback>> GetRecentByLocationAsync(decimal lat, decimal lon, double radiusKm, int hours, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow.AddHours(-Math.Abs(hours));
            var (minLat, maxLat, minLon, maxLon) = GeoBox.FromCenter(lat, lon, radiusKm);

            // Bounding-box pre-filter to leverage indexes and reduce dataset
            var query = _db.Feedbacks
                .AsNoTracking()
                .Where(f =>
                    f.CreatedAt >= since &&
                    f.Latitude >= minLat && f.Latitude <= maxLat &&
                    f.Longitude >= minLon && f.Longitude <= maxLon)
                .OrderByDescending(f => f.CreatedAt);

            // Note: We intentionally do not perform a precise haversine filter at DB level to keep provider-agnostic SQL.
            // If needed, a post-filter can be applied at the service layer.

            return await query.ToListAsync(ct);
        }

        public Task<List<Feedback>> GetByUserAsync(Guid userId, int skip = 0, int take = 50, CancellationToken ct = default)
        {
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 200);

            return _db.Feedbacks
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
    }

    internal sealed class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly AirWatchDbContext _db;

        public SearchHistoryRepository(AirWatchDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(SearchHistory entry, CancellationToken ct = default)
        {
            if (entry.Id == Guid.Empty)
                entry.Id = Guid.NewGuid();

            if (entry.SearchedAt == default)
                entry.SearchedAt = DateTime.UtcNow;

            await _db.SearchHistory.AddAsync(entry, ct);
            await _db.SaveChangesAsync(ct);
        }

        public Task<List<SearchHistory>> GetByUserAsync(Guid userId, int skip = 0, int take = 100, CancellationToken ct = default)
        {
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 500);

            return _db.SearchHistory
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.SearchedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
    }

    internal sealed class PollutionCacheRepository : IPollutionCacheRepository
    {
        private readonly AirWatchDbContext _db;

        public PollutionCacheRepository(AirWatchDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(PollutionCache cache, CancellationToken ct = default)
        {
            if (cache.Id == Guid.Empty)
                cache.Id = Guid.NewGuid();

            if (cache.FetchedAt == default)
                cache.FetchedAt = DateTime.UtcNow;

            await _db.PollutionCache.AddAsync(cache, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<PollutionCache?> GetLatestForLocationAsync(decimal lat, decimal lon, int maxAgeMinutes = 60, double radiusKm = 2.0, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow.AddMinutes(-Math.Abs(maxAgeMinutes));
            var (minLat, maxLat, minLon, maxLon) = GeoBox.FromCenter(lat, lon, radiusKm);

            return await _db.PollutionCache
                .AsNoTracking()
                .Where(p =>
                    p.FetchedAt >= since &&
                    p.Latitude >= minLat && p.Latitude <= maxLat &&
                    p.Longitude >= minLon && p.Longitude <= maxLon)
                .OrderByDescending(p => p.FetchedAt)
                .FirstOrDefaultAsync(ct);
        }

        public Task<List<PollutionCache>> GetHistoryAsync(decimal lat, decimal lon, int hours, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow.AddHours(-Math.Abs(hours));

            // Default implementation: exact coordinate match. If needed, callers can pass rounded coordinates.
            return _db.PollutionCache
                .AsNoTracking()
                .Where(p => p.Latitude == lat && p.Longitude == lon && p.FetchedAt >= since)
                .OrderBy(p => p.FetchedAt)
                .ToListAsync(ct);
        }
    }

    // Helpers

    internal static class GeoBox
    {
        // Approx conversions (sufficient for small radii and prefiltering)
        private const double KmPerDegreeLat = 111.0;

        public static (decimal minLat, decimal maxLat, decimal minLon, decimal maxLon) FromCenter(decimal lat, decimal lon, double radiusKm)
        {
            var latD = (double)lat;
            var lonD = (double)lon;

            var dLat = radiusKm / KmPerDegreeLat;

            var cosLat = Math.Cos(latD * Math.PI / 180.0);
            // Prevent division by zero close to poles; clamp cos to small epsilon
            cosLat = Math.Max(0.01, Math.Abs(cosLat));
            var dLon = radiusKm / (KmPerDegreeLat * cosLat);

            var minLat = latD - dLat;
            var maxLat = latD + dLat;
            var minLon = lonD - dLon;
            var maxLon = lonD + dLon;

            return ((decimal)minLat, (decimal)maxLat, (decimal)minLon, (decimal)maxLon);
        }
    }
}
