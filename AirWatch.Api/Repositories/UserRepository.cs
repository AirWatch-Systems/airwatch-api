using System;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Api.Repositories
{
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
}