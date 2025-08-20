using Microsoft.AspNetCore.Mvc;

namespace TarimHibe.Controllers
{
    public class MapsController : Controller
    {
        // GET: Maps
        public IActionResult Google()
        {
            return View();
        }
        public IActionResult Vector()
        {
            return View();
        }
    }
}