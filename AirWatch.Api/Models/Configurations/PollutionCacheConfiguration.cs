using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AirWatch.Api.Models.Entities;

namespace AirWatch.Api.Models.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the PollutionCache entity
    /// </summary>
    internal class PollutionCacheConfiguration : IEntityTypeConfiguration<PollutionCache>
    {
        public void Configure(EntityTypeBuilder<PollutionCache> builder)
        {
            // Table configuration
            builder.ToTable("PollutionCache");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties configuration
            builder.Property(x => x.Latitude)
                .HasPrecision(10, 8)
                .IsRequired();

            builder.Property(x => x.Longitude)
                .HasPrecision(11, 8)
                .IsRequired();

            builder.Property(x => x.AQI)
                .IsRequired();

            builder.Property(x => x.PM25)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.PM10)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.CO)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.NO2)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.SO2)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.O3)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.FetchedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            // Indexes
            builder.HasIndex(x => new { x.Latitude, x.Longitude, x.FetchedAt })
                .HasDatabaseName("IX_PollutionCache_Location_Time");
        }
    }
}
