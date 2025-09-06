namespace ShipmentTrackingSystem.Models
{
    public class ShipmentLocation
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = default!;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double? SpeedKmh { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
