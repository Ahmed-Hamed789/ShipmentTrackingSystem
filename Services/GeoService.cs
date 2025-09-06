using System;

namespace ShipmentTrackingSystem.Services
{
    public class GeoService : IGeoService
    {
        public double DistanceMeters(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371000; // meters
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        public bool InRadius(double lat, double lng, double centerLat, double centerLng, double radiusMeters)
            => DistanceMeters(lat, lng, centerLat, centerLng) <= radiusMeters;

        public string ShortAddress(string address, int max = 30)
            => address.Length <= max ? address : address[..max] + "...";

        private static double ToRad(double deg) => (Math.PI / 180) * deg;
    }
}
