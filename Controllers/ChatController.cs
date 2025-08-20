using Microsoft.AspNetCore.Mvc;

namespace TarimHibe.Controllers
{
    public class ChatController : Controller
    {
        // GET: Chat
        public IActionResult Index()
        {
            return View();
        }
    }
}