using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Api.Repositories
{
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
}