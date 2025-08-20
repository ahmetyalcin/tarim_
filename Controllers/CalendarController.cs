using Microsoft.AspNetCore.Mvc;

namespace TarimHibe.Controllers
{
    public class CalendarController : Controller
    {
        // GET: Calendar
        public IActionResult Index()
        {
            return View();
        }
    }
}