using System.ComponentModel.DataAnnotations;

namespace ShipmentTrackingSystem.Models
{
  

    public class Shipment
    {
        public int Id { get; set; }

        [Required, StringLength(16)]
        public string TrackingNumber { get; set; } = default!;

        [Required] public string OriginAddress { get; set; } = default!;
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }

        [Required] public string DestinationAddress { get; set; } = default!;
        public double DestLat { get; set; }
        public double DestLng { get; set; }

        [Required]
        public int DriverId { get; set; }
        public Driver Driver { get; set; } = default!;

        [Required]
        public int UserId { get; set; }
        public AppUser User { get; set; } = default!;

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;
        public string? StatusNote { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
