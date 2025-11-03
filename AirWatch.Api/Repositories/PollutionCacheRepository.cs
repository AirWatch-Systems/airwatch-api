using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Api.Repositories
{
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

            return _db.PollutionCache
                .AsNoTracking()
                .Where(p => p.Latitude == lat && p.Longitude == lon && p.FetchedAt >= since)
                .OrderBy(p => p.FetchedAt)
                .ToListAsync(ct);
        }
    }
}