using Microsoft.EntityFrameworkCore;
using AirWatch.Api.Models.Configurations;

namespace AirWatch.Api.Models.Extensions
{
    /// <summary>
    /// Extension methods for ModelBuilder to apply AirWatch entity configurations
    /// </summary>
    public static class AirWatchModelBuilderExtensions
    {
        /// <summary>
        /// Applies all AirWatch entity configurations to the model builder
        /// </summary>
        /// <param name="modelBuilder">The model builder instance</param>
        /// <returns>The model builder for method chaining</returns>
        public static ModelBuilder ApplyAirWatchEntityConfigurations(this ModelBuilder modelBuilder)
        {
            // Apply all entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new FeedbackConfiguration());

            return modelBuilder;
        }

        /// <summary>
        /// Applies all AirWatch entity configurations from the assembly
        /// This is an alternative method that automatically discovers all IEntityTypeConfiguration implementations
        /// </summary>
        /// <param name="modelBuilder">The model builder instance</param>
        /// <returns>The model builder for method chaining</returns>
        public static ModelBuilder ApplyAirWatchEntityConfigurationsFromAssembly(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AirWatchModelBuilderExtensions).Assembly,
                type => type.Namespace == "AirWatch.Api.Models.Configurations");

            return modelBuilder;
        }
    }
}
