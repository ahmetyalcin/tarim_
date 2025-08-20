using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public class HibelerController : BaseController
    {
        private readonly HibeDbContext _context;
        public HibelerController(MenuService menuService, HibeDbContext context)
            : base(menuService)
        {
            _context = context;
        }

        // Oturumdan user id çekiyoruz
        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

        // Listeleme (opsiyonel)
        // HibelerController.cs

        [HttpGet]
        public async Task<IActionResult> Index(bool? yayinda)
        {
            // Filtre uygula (null ise tüm kayıtlar)
            var query = _context.Hibeler.AsQueryable();
            if (yayinda.HasValue)
                query = query.Where(h => h.Yayinda == yayinda.Value);

            var model = await query
                .OrderByDescending(h => h.BaslangicTarihi)
                .ToListAsync();

            ViewBag.FilterYayinda = yayinda;
            return View(model);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleYayinda(int id)
        {
            var h = await _context.Hibeler.FindAsync(id);
            if (h == null)
                return Json(new { success = false, message = "Hibe bulunamadı." });

            h.Yayinda = !h.Yayinda;
            await _context.SaveChangesAsync();

            return Json(new { success = true, yayinda = h.Yayinda });
        }



        // GET: Yeni başvuru formu
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Kayıt et
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Hibe model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // zorunlu defaultlar
            model.UserID = CurrentUserId;
            model.KayitTarihi = DateTime.Now;
            model.Yayinda = true;
            //            model.OdemeDurumu = false;
            //          model.CKSOnayDurumu = false;
            //        model.AdminOnayDurumu = false;

            _context.Hibeler.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hibe başvurunuz alındı.";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Hibeler.FindAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // POST: /Hibeler/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hibe model)
        {
            if (id != model.HibeID)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var hibe = await _context.Hibeler.FindAsync(id);
            if (hibe == null)
                return NotFound();

            // sadece Add'de set ettiğiniz alanları güncelliyoruz
            hibe.HibeTuru = model.HibeTuru;
            hibe.Miktar = model.Miktar;
            hibe.UcretliMi = model.UcretliMi;
            hibe.HibeAciklama = model.HibeAciklama;
            hibe.BaslangicTarihi = model.BaslangicTarihi;
            hibe.BitisTarihi = model.BitisTarihi;
            // model.UserID, KayitTarihi, Yayinda vs. dokunmuyoruz.

            try
            {
                _context.Update(hibe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hibe başarıyla güncellendi.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Güncelleme sırasında bir hata oluştu.";
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // … geri kalan Index, Add, ToggleYayinda vs. …
    }

}

