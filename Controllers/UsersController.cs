using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentTrackingSystem.Data;
using ShipmentTrackingSystem.Filters;
using ShipmentTrackingSystem.Models;

namespace ShipmentTrackingSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        { ViewBag.ReturnUrl = returnUrl; return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            email = (email ?? "").Trim().ToLowerInvariant();
            var typedHash = Hash(password ?? string.Empty);

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("email", "Email is required.");
            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "Password is required.");

            if (!ModelState.IsValid)
            { ViewBag.ReturnUrl = returnUrl; return View(); }

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !string.Equals(user.PasswordHash, typedHash, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl; return View();
            }

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("userEmail", user.Email);

            if (!string.IsNullOrWhiteSpace(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(MyShipments));
        }

        [HttpGet]
        public IActionResult Register()
        { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            email = (email ?? "").Trim().ToLowerInvariant();
            fullName = (fullName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                ModelState.AddModelError("email", "Valid email is required.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                ModelState.AddModelError("password", "Password must be at least 6 characters.");

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
                ModelState.AddModelError("email", "Email is already registered.");

            if (!ModelState.IsValid) return View();

            var user = new AppUser { Email = email, PasswordHash = Hash(password), FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("userEmail", user.Email);

            return RedirectToAction(nameof(MyShipments));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("userId");
            HttpContext.Session.Remove("userEmail");
            return RedirectToAction(nameof(Login));
        }

        [TrackAuthorize]
        public async Task<IActionResult> MyShipments()
        {
            var uid = HttpContext.Session.GetInt32("userId")!.Value;
            var list = await _db.Shipments
                .Include(s => s.Driver)
                .Where(s => s.UserId == uid)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            return View(list);
        }

        private static string Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
