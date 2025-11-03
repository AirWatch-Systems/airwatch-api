using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Api.Repositories
{
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

            var query = _db.Feedbacks
                .AsNoTracking()
                .Where(f =>
                    f.CreatedAt >= since &&
                    f.Latitude >= minLat && f.Latitude <= maxLat &&
                    f.Longitude >= minLon && f.Longitude <= maxLon)
                .OrderByDescending(f => f.CreatedAt);

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

        public async Task<Feedback?> GetLatestByUserInRegionAsync(Guid userId, decimal lat, decimal lon, double radiusKm, CancellationToken ct = default)
        {
            var (minLat, maxLat, minLon, maxLon) = GeoBox.FromCenter(lat, lon, radiusKm);

            return await _db.Feedbacks
                .AsNoTracking()
                .Where(f => f.UserId == userId &&
                           f.Latitude >= minLat && f.Latitude <= maxLat &&
                           f.Longitude >= minLon && f.Longitude <= maxLon)
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}