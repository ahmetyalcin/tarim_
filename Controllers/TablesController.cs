using Microsoft.AspNetCore.Mvc;
using TarimHibe.Models;

namespace TarimHibe.Controllers
{
    public class TablesController : Controller
    {
        // GET: Tables
        public IActionResult BasicTables()
        {
            return View();
        }
        public IActionResult DataTables()
        {
            return View();
        }
        public IActionResult Editable()
        {
            return View();
        }

        // GET: ekranda formu boş modelle açmak için
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new TarimAraziViewModel1();
            return View("Editable", vm);  // eğer View adı Editable.cshtml ise
        }

        // POST: form gönderildiğinde
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TarimAraziViewModel1 model)
        {
            if (!ModelState.IsValid)
            {
                // validation hatası varsa tekrar aynı view’e model’i geri gönder
                return View("Editable", model);
            }

            // TODO: Kaydetme işlemi
            // ...

            return RedirectToAction("Index");
        }

    }
}