using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AirWatch.Api.Models.Entities;

namespace AirWatch.Api.Models.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the SearchHistory entity
    /// </summary>
    internal class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
    {
        public void Configure(EntityTypeBuilder<SearchHistory> builder)
        {
            // Table configuration
            builder.ToTable("SearchHistory");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties configuration
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.LocationName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("varchar(255)");

            builder.Property(x => x.Latitude)
                .HasPrecision(10, 8);

            builder.Property(x => x.Longitude)
                .HasPrecision(11, 8);

            builder.Property(x => x.SearchedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_SearchHistory_UserId");

            builder.HasIndex(x => x.SearchedAt)
                .HasDatabaseName("IX_SearchHistory_SearchedAt");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
