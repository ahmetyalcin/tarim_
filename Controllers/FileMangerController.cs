using Microsoft.AspNetCore.Mvc;

namespace TarimHibe.Controllers
{
    public class FileMangerController : Controller
    {
        // GET: FileManger
        public IActionResult Index()
        {
            return View();
        }
    }
}