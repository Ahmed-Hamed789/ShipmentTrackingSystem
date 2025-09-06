namespace ShipmentTrackingSystem.Services
{
    public interface IGeoService
    {
        double DistanceMeters(double lat1, double lng1, double lat2, double lng2);
        bool InRadius(double lat, double lng, double centerLat, double centerLng, double radiusMeters);
        string ShortAddress(string address, int max = 30);
    }
}
