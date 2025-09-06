using Microsoft.AspNetCore.Mvc;

namespace ShipmentTrackingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _cfg;
        public AdminController(IConfiguration cfg) { _cfg = cfg; }

        public IActionResult Login(string? returnUrl = null)
        { ViewBag.ReturnUrl = returnUrl; return View(); }

        [HttpPost]
        public IActionResult Login(string username, string password, string? returnUrl = null)
        {
            var u = _cfg["Admin:Username"]; var p = _cfg["Admin:Password"];
            if (username == u && password == p)
            {
                HttpContext.Session.SetString("isAdmin", "1");
                return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/Shipments" : returnUrl!);
            }
            ViewBag.ReturnUrl = returnUrl; ViewBag.Err = "Invalid credentials.";
            return View();
        }

        public IActionResult Logout()
        { HttpContext.Session.Remove("isAdmin"); return Redirect("/"); }
    }
}
