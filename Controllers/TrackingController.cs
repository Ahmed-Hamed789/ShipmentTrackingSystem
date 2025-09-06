using Microsoft.AspNetCore.Mvc;
using ShipmentTrackingSystem.Services;

namespace ShipmentTrackingSystem.Controllers
{
    public class TrackingController : Controller
    {
        private readonly IShipmentService _svc;
        private readonly IConfiguration _cfg;
        public TrackingController(IShipmentService svc, IConfiguration cfg) { _svc = svc; _cfg = cfg; }

        public IActionResult Find() => View();

        [HttpPost]
        public IActionResult Find(string trackingNumber)
        {
            var tn = (trackingNumber ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(tn)) { ViewBag.Err = "Enter tracking number."; return View(); }
            return Redirect($"/t/{tn}");
        }

        public async Task<IActionResult> Index(string id)
        {
            var s = await _svc.GetByTrackingAsync(id);
            if (s == null) return NotFound();
            ViewBag.GoogleMapsKey = _cfg["GoogleMaps:ApiKey"] ?? string.Empty;
            return View(s);
        }
    }
}
