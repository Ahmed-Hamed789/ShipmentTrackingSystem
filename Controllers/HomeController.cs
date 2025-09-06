using Microsoft.AspNetCore.Mvc;

namespace ShipmentTrackingSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
