using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentTrackingSystem.Data;
using ShipmentTrackingSystem.Filters;
using ShipmentTrackingSystem.Models;
using ShipmentTrackingSystem.Services;

namespace ShipmentTrackingSystem.Controllers
{
    [AdminAuthorize]
    public class ShipmentsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IShipmentService _svc;

        public ShipmentsController(AppDbContext db, IShipmentService svc)
        {
            _db = db;
            _svc = svc;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _db.Shipments
                .Include(s => s.Driver)
                .Include(s => s.User)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewBag.Drivers = await _db.Drivers.OrderBy(d => d.NationalId).ToListAsync();
            ViewBag.Users = await _db.Users.OrderBy(u => u.Email).ToListAsync();
            return View(list);

        }

        public async Task<IActionResult> Create()
        {
            var defaultUserId = await _db.Users
    .Where(u => u.Email == "user@demo.local")
    .Select(u => u.Id)
    .FirstAsync();

            ViewBag.DefaultUserId = defaultUserId;

            ViewBag.Drivers = await _db.Drivers.OrderBy(d => d.NationalId).ToListAsync();
            ViewBag.Users = await _db.Users.OrderBy(u => u.Email).ToListAsync();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(
            string originAddress, double originLat, double originLng,
            string destinationAddress, double destLat, double destLng,
            int driverId, int userId)
        {
            if (driverId <= 0) ModelState.AddModelError("driverId", "Driver is required.");
            if (userId <= 0) ModelState.AddModelError("userId", "User is required.");
            if (!ModelState.IsValid)
            {
                ViewBag.Drivers = await _db.Drivers.OrderBy(d => d.NationalId).ToListAsync();
                ViewBag.Users = await _db.Users.OrderBy(u => u.Email).ToListAsync();
                return View();
            }

            var s = await _svc.CreateShipmentAsync(
                originAddress, originLat, originLng,
                destinationAddress, destLat, destLng,
                driverId, userId);

            TempData["msg"] = $"Created {s.TrackingNumber}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int shipmentId, int driverId)
        {
            try
            {
                await _svc.AssignOrChangeDriverAsync(shipmentId, driverId);
                TempData["msg"] = "Driver saved.";
            }
            catch (Exception ex)
            {
                TempData["err"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditLocation(int id)
        {
            var s = await _db.Shipments.FindAsync(id);
            if (s == null) return NotFound();
            return View(s);
        }

        [HttpPost]
        public async Task<IActionResult> EditLocation(int id, double lat, double lng, double? speedKmh, string? note)
        {
            var r = await _svc.UpdateCurrentLocationAsync(id, lat, lng, speedKmh, note);
            if (!r.ok) TempData["err"] = r.error;
            else TempData["msg"] = "Location updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            await _svc.CancelShipmentAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _svc.DeleteShipmentAsync(id);
                TempData["msg"] = "Shipment deleted.";
            }
            catch (Exception ex)
            {
                TempData["err"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
