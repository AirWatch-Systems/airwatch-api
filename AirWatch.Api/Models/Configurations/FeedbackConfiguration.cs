using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AirWatch.Api.Models.Entities;

namespace AirWatch.Api.Models.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the Feedback entity
    /// </summary>
    internal class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            // Table configuration with check constraint (new syntax)
            builder.ToTable("Feedbacks", t =>
            {
                t.HasCheckConstraint("CK_Feedbacks_Rating_Range", "[Rating] BETWEEN 1 AND 5");
            });

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties configuration
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Latitude)
                .HasPrecision(10, 8);

            builder.Property(x => x.Longitude)
                .HasPrecision(11, 8);

            builder.Property(x => x.Rating)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => new { x.Latitude, x.Longitude })
                .HasDatabaseName("IX_Feedbacks_Location");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("IX_Feedbacks_CreatedAt");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
