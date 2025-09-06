using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShipmentTrackingSystem.Data;
using ShipmentTrackingSystem.Hubs;
using ShipmentTrackingSystem.Models;

namespace ShipmentTrackingSystem.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly AppDbContext _db;
        private readonly IGeoService _geo;
        private readonly IHubContext<TrackingHub> _hub;

        private const double ORIGIN_RADIUS_M = 500;
        private const double DEST_RADIUS_M = 500;
        private const double MOVED_FROM_ORIGIN_M = 200;
        private const double OFFROUTE_CORRIDOR_M = 40000;

        public ShipmentService(AppDbContext db, IGeoService geo, IHubContext<TrackingHub> hub)
        {
            _db = db; _geo = geo; _hub = hub;
        }

        public async Task<Shipment> CreateShipmentAsync(
            string originAddress, double originLat, double originLng,
            string destinationAddress, double destLat, double destLng,
            int driverId, int userId)
        {
            var driver = await _db.Drivers.FindAsync(driverId);
            var user = await _db.Users.FindAsync(userId);
            if (driver == null) throw new InvalidOperationException("Driver required.");
            if (user == null) throw new InvalidOperationException("User required.");

            var tracking = await GenerateUniqueTrackingAsync();

            var shipment = new Shipment
            {
                TrackingNumber = tracking,
                OriginAddress = originAddress,
                OriginLat = originLat,
                OriginLng = originLng,
                DestinationAddress = destinationAddress,
                DestLat = destLat,
                DestLng = destLng,
                DriverId = driverId,
                UserId = userId,
                Status = ShipmentStatus.Assigned,
                StatusNote = "Assigned to driver"
            };

            _db.Shipments.Add(shipment);
            await _db.SaveChangesAsync();
            return shipment;
        }

        public async Task<Shipment?> AssignOrChangeDriverAsync(int shipmentId, int driverId)
        {
            var shipment = await _db.Shipments.Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == shipmentId);
            var driver = await _db.Drivers.FindAsync(driverId);
            if (shipment == null || driver == null) return null;
            if (shipment.Status is ShipmentStatus.InTransit or ShipmentStatus.Delivered or ShipmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot change driver now.");

            shipment.DriverId = driverId;
            shipment.Status = ShipmentStatus.Assigned;
            shipment.StatusNote = "Assigned to driver";
            shipment.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await Broadcast(shipment);
            return shipment;
        }

        public async Task<Driver?> UpdateDriverPhoneAsync(int driverId, string newPhone)
        {
            var d = await _db.Drivers.FindAsync(driverId);
            if (d == null) return null;
            d.Phone = newPhone;
            await _db.SaveChangesAsync();
            return d;
        }

        public async Task<bool> CancelShipmentAsync(int shipmentId, string? reason = null)
        {
            var s = await _db.Shipments.FindAsync(shipmentId);
            if (s == null) return false;
            if (s.Status == ShipmentStatus.Delivered) return false;

            s.Status = ShipmentStatus.Cancelled;
            s.CancelledAt = DateTime.UtcNow;
            s.StatusNote = string.IsNullOrWhiteSpace(reason) ? "Cancelled" : $"Cancelled: {reason}";
            s.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await Broadcast(s);
            return true;
        }

        public async Task<bool> DeleteShipmentAsync(int shipmentId)
        {
            var s = await _db.Shipments.FindAsync(shipmentId);
            if (s == null) return false;
            if (s.Status is not (ShipmentStatus.Delivered or ShipmentStatus.Cancelled))
                throw new InvalidOperationException("Only delivered or cancelled shipments can be deleted.");
            _db.Shipments.Remove(s);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<Shipment?> GetByTrackingAsync(string trackingNumber)
        {
            var tn = (trackingNumber ?? string.Empty).Trim().ToUpperInvariant();
            return _db.Shipments.Include(x => x.Driver).Include(x => x.User).FirstOrDefaultAsync(x => x.TrackingNumber == tn);
        }

        public Task<(bool ok, Shipment? shipment, string? error)> UpdateCurrentLocationAsync(
            int shipmentId, double lat, double lng, double? speedKmh = null, string? note = null)
        {
            return UpdateLocationCore(_db.Shipments.FirstOrDefaultAsync(s => s.Id == shipmentId), lat, lng, speedKmh, note);
        }

        public Task<(bool ok, Shipment? shipment, string? error)> UpdateLocationAsync(
            string trackingNumber, double lat, double lng, double? speedKmh = null, string? note = null)
        {
            return UpdateLocationCore(GetByTrackingAsync(trackingNumber), lat, lng, speedKmh, note);
        }

        private async Task<(bool ok, Shipment? shipment, string? error)> UpdateLocationCore(
            Task<Shipment?> shipmentTask, double lat, double lng, double? speedKmh, string? note)
        {
            var s = await shipmentTask;
            if (s == null) return (false, null, "Shipment not found.");
            if (s.Status is ShipmentStatus.Delivered or ShipmentStatus.Cancelled)
                return (false, s, "Shipment already completed/cancelled; location updates are blocked.");

            _db.ShipmentLocations.Add(new ShipmentLocation { ShipmentId = s.Id, Lat = lat, Lng = lng, SpeedKmh = speedKmh, Note = note });

            var now = DateTime.UtcNow;
            var nearOrigin = _geo.InRadius(lat, lng, s.OriginLat, s.OriginLng, ORIGIN_RADIUS_M);
            var nearDest = _geo.InRadius(lat, lng, s.DestLat, s.DestLng, DEST_RADIUS_M);
            var movedFromOrigin = _geo.DistanceMeters(lat, lng, s.OriginLat, s.OriginLng) >= MOVED_FROM_ORIGIN_M;

            var offRoute = false;
            if (!nearOrigin && !nearDest)
            {
                var dSeg = DistanceFromSegmentMeters(lat, lng, s.OriginLat, s.OriginLng, s.DestLat, s.DestLng);
                offRoute = dSeg > OFFROUTE_CORRIDOR_M;
            }

            if (offRoute && s.Status is ShipmentStatus.Assigned or ShipmentStatus.InTransit)
            {
                s.Status = ShipmentStatus.Cancelled;
                s.CancelledAt = now;
                s.StatusNote = "Sorry, the driver went off route";
            }
            else if (nearDest)
            {
                s.Status = ShipmentStatus.Delivered;
                s.CompletedAt = now;
                s.StatusNote = $"Arrived at {s.DestinationAddress}";
            }
            else
            {
                if (s.Status == ShipmentStatus.Assigned && (movedFromOrigin || nearOrigin))
                {
                    s.Status = ShipmentStatus.InTransit;
                    s.StatusNote = $"Between {s.OriginAddress} and {s.DestinationAddress}";
                }
                if (s.Status == ShipmentStatus.Created && (movedFromOrigin || nearOrigin))
                {
                    s.Status = ShipmentStatus.InTransit;
                    s.StatusNote = $"Between {s.OriginAddress} and {s.DestinationAddress}";
                }
            }

            s.UpdatedAt = now;
            await _db.SaveChangesAsync();
            await Broadcast(s, lat, lng);
            return (true, s, null);
        }

        private async Task Broadcast(Shipment s, double? lat = null, double? lng = null)
        {
            if (s.Driver == null) s.Driver = await _db.Drivers.FindAsync(s.DriverId);
            await _hub.Clients.Group(s.TrackingNumber).SendAsync("TrackingUpdate", new
            {
                trackingNumber = s.TrackingNumber,
                status = s.Status.ToString(),
                statusNote = s.StatusNote,
                lat,
                lng,
                driverNationalId = s.Driver?.NationalId,
                driverPhone = s.Driver?.Phone,
                ts = DateTime.UtcNow
            });
        }

        private async Task<string> GenerateUniqueTrackingAsync()
        {
            var rnd = new Random();
            while (true)
            {
                var candidate = "EG" + rnd.Next(100000, 999999);
                var exists = await _db.Shipments.AnyAsync(s => s.TrackingNumber == candidate);
                if (!exists) return candidate;
            }
        }

        private static double DistanceFromSegmentMeters(double plat, double plng, double alat, double alng, double blat, double blng)
        {
            const double R = 6371000.0;
            double lat0 = Deg2Rad((alat + blat) / 2.0);
            (double ax, double ay) = Project(alat, alng, lat0, R);
            (double bx, double by) = Project(blat, blng, lat0, R);
            (double px, double py) = Project(plat, plng, lat0, R);
            double vx = bx - ax, vy = by - ay;
            double wx = px - ax, wy = py - ay;
            double c1 = vx * wx + vy * wy;
            if (c1 <= 0) return Math.Sqrt(wx * wx + wy * wy);
            double c2 = vx * vx + vy * vy;
            if (c2 <= c1) return Math.Sqrt((px - bx) * (px - bx) + (py - by) * (py - by));
            double t = c1 / c2;
            double cx = ax + t * vx, cy = ay + t * vy;
            double dx = px - cx, dy = py - cy;
            return Math.Sqrt(dx * dx + dy * dy);

            static (double x, double y) Project(double lat, double lng, double lat0, double R)
            { double phi = Deg2Rad(lat); double lam = Deg2Rad(lng); return (R * lam * Math.Cos(lat0), R * phi); }
            static double Deg2Rad(double d) => (Math.PI / 180.0) * d;
        }
    }
}
