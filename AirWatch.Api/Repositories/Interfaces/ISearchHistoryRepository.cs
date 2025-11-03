using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;

namespace AirWatch.Api.Repositories
{
    public interface ISearchHistoryRepository
    {
        Task AddAsync(SearchHistory entry, CancellationToken ct = default);
        Task<List<SearchHistory>> GetByUserAsync(Guid userId, int skip = 0, int take = 100, CancellationToken ct = default);
    }
}