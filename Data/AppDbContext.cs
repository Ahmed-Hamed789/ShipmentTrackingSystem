using Microsoft.EntityFrameworkCore;
using ShipmentTrackingSystem.Models;

namespace ShipmentTrackingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentLocation> ShipmentLocations => Set<ShipmentLocation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Shipment>().HasIndex(s => s.TrackingNumber).IsUnique();
            modelBuilder.Entity<Driver>().HasIndex(d => d.NationalId).IsUnique();
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<ShipmentLocation>()
                .HasOne(sl => sl.Shipment)
                .WithMany()
                .HasForeignKey(sl => sl.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Driver)
                .WithMany()
                .HasForeignKey(s => s.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
