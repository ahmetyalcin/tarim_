using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;
using TarimHibe.Services; // İzmirBBService


namespace TarimHibe.Controllers
{
    public class EkipmanlarController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;

        public EkipmanlarController(MenuService menuService, HibeDbContext context,IzmirBBService izmirBBService) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
        }

        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
        public IActionResult Index()
        {
           var ekipmanlar = _context.Ekipmanlar
          .Where(e => e.UserID == CurrentUserId)
          .Include(e => e.EkipmanTurleri)
          .ToList();
            return View(ekipmanlar);
        }

        public async Task<IActionResult> Add()
        {
            ViewBag.EkipmanTurleri = _context.EkipmanTurleri
                .Select(x => new SelectListItem { Value = x.TurID.ToString(), Text = x.TurAdi }).ToList();

            var ilceler = await _izmirBBService.GetIlcelerAsync();
            return View(ilceler);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ekipman = _context.Ekipmanlar.Find(id);
            if (ekipman == null || ekipman.UserID != CurrentUserId)
                return Json(new { success = false, message = "Kayıt bulunamadı veya yetkisiz erişim." });

            _context.Ekipmanlar.Remove(ekipman);
            _context.SaveChanges();

            return Json(new { success = true, message = "Ekipman silindi." });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ekipman = _context.Ekipmanlar.FirstOrDefault(x => x.EkipmanID == id && x.UserID == CurrentUserId);
            if (ekipman == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            ViewBag.EkipmanTurleri = _context.EkipmanTurleri.Select(x => new SelectListItem
            {
                Value = x.TurID.ToString(),
                Text = x.TurAdi,
                Selected = x.TurID == ekipman.TurID
            }).ToList();

            var ilceler = await _izmirBBService.GetIlcelerAsync();
            ViewBag.Ekipman = ekipman;

            if (!string.IsNullOrEmpty(ekipman.Ilce))
            {
                var mahalleler = await _izmirBBService.GetMahallelerAsync(ekipman.Ilce);
                ViewBag.Mahalleler = mahalleler;
            }

            return View(ilceler);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Add(Ekipmanlar model)
        {
            foreach (var error in ModelState)
            {
                foreach (var subError in error.Value.Errors)
                {
                    Console.WriteLine($"Property: {error.Key}, Error: {subError.ErrorMessage}");
                }
            }


            ModelState.Remove("EkipmanTurleri");
            ModelState.Remove("Users");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurunuz.";
                return RedirectToAction("Add");
            }

            model.UserID = CurrentUserId;
            model.KayitTarihi = DateTime.Now;

            _context.Ekipmanlar.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Ekipman başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Ekipmanlar model)
        {
            var ekip = _context.Ekipmanlar.FirstOrDefault(x => x.EkipmanID == model.EkipmanID && x.UserID == CurrentUserId);
            ModelState.Remove("EkipmanTurleri");
            ModelState.Remove("Users");

            ekip.UserID = CurrentUserId;
            ekip.KayitTarihi = DateTime.Now;
            ekip.EkipmanAdi = model.EkipmanAdi;
            ekip.TurID = model.TurID;
            ekip.Plaka = model.Plaka;
            ekip.Marka = model.Marka;
            ekip.Modeli = model.Modeli;
            ekip.Ilce = model.Ilce;
            ekip.Mahalle = model.Mahalle;
            ekip.Adet = model.Adet;
            ekip.Durum = model.Durum;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurunuz.";
                return RedirectToAction("Edit", new { id = model.EkipmanID });
            }

            if (ekip == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

          

            

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Ekipman başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
        

        [HttpGet]
        public async Task<IActionResult> GetMahalleler(string ilceId)
        {
            try
            {
                var mahalleler = await _izmirBBService.GetMahallelerAsync(ilceId);
                return Json(mahalleler);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Mahalleler alınamadı: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _context.Dispose();

            base.Dispose(disposing);
        }
    }
}
