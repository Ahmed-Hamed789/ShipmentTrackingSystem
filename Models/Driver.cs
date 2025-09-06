using System.ComponentModel.DataAnnotations;

namespace ShipmentTrackingSystem.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required, StringLength(14, MinimumLength = 14)]
        [RegularExpression(@"^\d{14}$")]
        public string NationalId { get; set; } = default!;

        [Required, RegularExpression(@"^\+?\d{7,15}$")]
        public string Phone { get; set; } = default!;

        public bool IsActive { get; set; } = true;
    }
}
