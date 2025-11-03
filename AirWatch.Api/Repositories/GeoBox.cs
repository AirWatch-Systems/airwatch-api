using System;

namespace AirWatch.Api.Repositories
{
    internal static class GeoBox
    {
        private const double KmPerDegreeLat = 111.0;

        public static (decimal minLat, decimal maxLat, decimal minLon, decimal maxLon) FromCenter(decimal lat, decimal lon, double radiusKm)
        {
            var latD = (double)lat;
            var lonD = (double)lon;

            var dLat = radiusKm / KmPerDegreeLat;

            var cosLat = Math.Cos(latD * Math.PI / 180.0);
            cosLat = Math.Max(0.01, Math.Abs(cosLat));
            var dLon = radiusKm / (KmPerDegreeLat * cosLat);

            var minLat = latD - dLat;
            var maxLat = latD + dLat;
            var minLon = lonD - dLon;
            var maxLon = lonD + dLon;

            return ((decimal)minLat, (decimal)maxLat, (decimal)minLon, (decimal)maxLon);
        }
    }
}