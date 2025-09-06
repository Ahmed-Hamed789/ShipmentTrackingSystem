using System.ComponentModel.DataAnnotations;

namespace ShipmentTrackingSystem.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = default!;

        [Required, StringLength(256)]
        public string PasswordHash { get; set; } = default!;

        [StringLength(120)]
        public string? FullName { get; set; }
    }
}
