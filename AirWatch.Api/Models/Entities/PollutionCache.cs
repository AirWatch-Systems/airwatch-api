using System;

namespace AirWatch.Api.Models.Entities
{
    /// <summary>
    /// Represents cached pollution data for a specific location
    /// </summary>
    public class PollutionCache
    {
        public Guid Id { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public int AQI { get; set; }
        public decimal PM25 { get; set; }
        public decimal PM10 { get; set; }
        public decimal CO { get; set; }
        public decimal NO2 { get; set; }
        public decimal SO2 { get; set; }
        public decimal O3 { get; set; }

        public DateTime FetchedAt { get; set; }
    }
}
