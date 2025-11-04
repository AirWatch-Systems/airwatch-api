using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.Models.Entities;
using AirWatch.Api.Models.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Api
{
    /// <summary>
    /// Primary Entity Framework Core DbContext for the AirWatch API.
    /// - Registers DbSets for all domain entities
    /// - Applies entity configurations (Fluent API) defined in Models/Entities.cs
    /// - Normalizes timestamps on insert/update
    /// </summary>
    public class AirWatchDbContext : DbContext
    {
        public AirWatchDbContext(DbContextOptions<AirWatchDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from Models/Entities.cs
            modelBuilder.ApplyAirWatchEntityConfigurations();
        }

        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Ensures CreatedAt/UpdatedAt semantics are respected on tracked entities.
        /// We prefer UTC time to match database defaults (GETUTCDATE()).
        /// </summary>
        private void ApplyTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Keep original CreatedAt, only bump UpdatedAt
                    entry.Property(p => p.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                }
            }

            foreach (var entry in ChangeTracker.Entries<Feedback>().Where(e => e.State == EntityState.Added))
            {
                // Only set CreatedAt if it hasn't been set (let DB default otherwise)
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
            }


        }
    }
}
