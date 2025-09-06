using ShipmentTrackingSystem.Models;

namespace ShipmentTrackingSystem.Services
{
    public interface IShipmentService
    {
        Task<Shipment> CreateShipmentAsync(
            string originAddress, double originLat, double originLng,
            string destinationAddress, double destLat, double destLng,
            int driverId, int userId);

        Task<Shipment?> AssignOrChangeDriverAsync(int shipmentId, int driverId);

        Task<bool> CancelShipmentAsync(int shipmentId, string? reason = null);
        Task<bool> DeleteShipmentAsync(int shipmentId);

        Task<Shipment?> GetByTrackingAsync(string trackingNumber);

        Task<(bool ok, Shipment? shipment, string? error)> UpdateLocationAsync(
            string trackingNumber, double lat, double lng, double? speedKmh = null, string? note = null);

        Task<(bool ok, Shipment? shipment, string? error)> UpdateCurrentLocationAsync(
            int shipmentId, double lat, double lng, double? speedKmh = null, string? note = null);

        Task<Driver?> UpdateDriverPhoneAsync(int driverId, string newPhone);
    }
}
