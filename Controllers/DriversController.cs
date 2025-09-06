using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentTrackingSystem.Data;
using ShipmentTrackingSystem.Filters;
using ShipmentTrackingSystem.Models;

namespace ShipmentTrackingSystem.Controllers
{
    [AdminAuthorize]
    public class DriversController : Controller
    {
        private readonly AppDbContext _db;
        public DriversController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var list = await _db.Drivers.OrderByDescending(d => d.Id).ToListAsync();
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nationalId, string phone)
        {
            if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 14 || !nationalId.All(char.IsDigit))
                ModelState.AddModelError("nationalId", "National ID must be 14 digits.");

            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 10 || phone.Length > 15)
                ModelState.AddModelError("phone", "Invalid phone number.");

            if (!ModelState.IsValid)
            {
                ViewData["nationalId"] = nationalId;
                ViewData["phone"] = phone;
                return View();
            }

            _db.Drivers.Add(new Driver { NationalId = nationalId, Phone = phone, IsActive = true });
            await _db.SaveChangesAsync();
            TempData["msg"] = "Driver created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditPhone(int id)
        {
            var d = await _db.Drivers.FindAsync(id);
            if (d == null) return NotFound();
            return View(d); // يعرض صفحة تعديل الهاتف
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhone(int id, string phone)
        {
            var d = await _db.Drivers.FindAsync(id);
            if (d == null) return NotFound();

            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 10 || phone.Length > 15)
            {
                ModelState.AddModelError("phone", "Invalid phone number.");
                return View(d);
            }

            d.Phone = phone;
            await _db.SaveChangesAsync();
            TempData["msg"] = "Phone updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _db.Drivers.FindAsync(id);
            if (d == null) return RedirectToAction(nameof(Index));

            // بما أن DriverId غير قابل للـ null، لا يمكن حذف السائق إذا كان له شحنات مرتبطة
            var hasAny = await _db.Shipments.AnyAsync(s => s.DriverId == id);
            if (hasAny)
            {
                TempData["err"] = "Cannot delete driver because there are shipments linked to this driver.";
                return RedirectToAction(nameof(Index));
            }

            _db.Drivers.Remove(d);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Driver deleted.";
            return RedirectToAction(nameof(Index));
        }

    }
}
