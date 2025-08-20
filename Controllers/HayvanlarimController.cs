using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TarimHibe.Models;
using TarimHibe.Models.ApiModels;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public class HayvanlarimController : BaseController
    {


        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;

        public HayvanlarimController(MenuService menuService, HibeDbContext context, IzmirBBService izmirBBService) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
        }



        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
        public IActionResult Index()
        {
            var Hayvanlar = _context.Hayvanlarim
           .Where(e => e.UserID == CurrentUserId)
           .Include(e => e.HayvanTuruGruplari)
           .Include(e => e.HayvanAltGruplari)
           .Include(e => e.HayvanTurleri)
           .ToList();
            return View(Hayvanlar);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var hayvan = _context.Hayvanlarim.Find(id);
                if (hayvan == null)
                {
                    return Json(new { success = false, message = "Kayıt bulunamadı." });
                }

                if (hayvan.UserID != CurrentUserId)
                {
                    return Json(new { success = false, message = "Bu kaydı silme yetkiniz yok." });
                }

                _context.Hayvanlarim.Remove(hayvan);
                _context.SaveChanges();

                return Json(new { success = true, message = "Kayıt başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme işlemi sırasında bir hata oluştu: " + ex.Message });
            }
        }

        public async Task<IActionResult> GetIlceler()
        {
            try
            {
                var ilceler = await _izmirBBService.GetIlcelerAsync();
                return Json(ilceler);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"İlçeler alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        public async Task<IActionResult> GetMahalleler(string ilceId)
        {
            try
            {
                var mahalleler = await _izmirBBService.GetMahallelerAsync(ilceId);
                return Json(mahalleler);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Mahalleler alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        public IActionResult GetHayvanAltGruplari(int grupId)
        {
            try
            {
                var altGruplar = _context.HayvanAltGruplari
                    .Where(x => x.GrupID == grupId)
                    .Select(x => new { id = x.AltGrupID, text = x.AltGrupAdi })
                    .ToList();
                return Json(altGruplar);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Alt gruplar alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        public IActionResult GetHayvanTurleri(int altGrupId)
        {
            try
            {
                var turler = _context.HayvanTurleri
                    .Where(x => x.AltGrupID == altGrupId)
                    .Select(x => new { id = x.TurID, text = x.TurAdi })
                    .ToList();
                return Json(turler);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Türler alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Add()
        {
            try
            {
                ViewBag.HayvanTuruGruplari = _context.HayvanTuruGruplari
                    .Select(x => new SelectListItem { Value = x.GrupID.ToString(), Text = x.GrupAdi })
                    .ToList();

                var ilceler = await _izmirBBService.GetIlcelerAsync();
                return View(ilceler);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Veriler yüklenirken bir hata oluştu: {ex.Message}";
                return View(new List<IlceModel>());
            }
        }
      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Hayvanlarim model)
        {
     
            ModelState.Remove("HayvanTurleri");
            ModelState.Remove("HayvanAltGruplari");
            ModelState.Remove("HayvanTuruGruplari");
            ModelState.Remove("Users");
            model.UserID = CurrentUserId;
            model.KayitTarihi = DateTime.Now;
            ModelState.Remove("Durum");
            model.Durum = "Aktif";
            model.Il = "0";

            foreach (var item in ModelState)
            {
                foreach (var error in item.Value.Errors)
                {
                    Console.WriteLine($"{item.Key}: {error.ErrorMessage}");
                }
            }



            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurunuz.";
                return RedirectToAction("Add");
            }

            _context.Hayvanlarim.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Ekipman başarıyla eklendi.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var hayvan = _context.Hayvanlarim
                    .FirstOrDefault(h => h.Id == id && h.UserID == CurrentUserId);

                if (hayvan == null)
                {
                    TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                    return RedirectToAction("Index");
                }

                ViewBag.HayvanTuruGruplari = _context.HayvanTuruGruplari
                    .Select(x => new SelectListItem
                    {
                        Value = x.GrupID.ToString(),
                        Text = x.GrupAdi,
                        Selected = x.GrupID == hayvan.HayGrupID
                    }).ToList();

                ViewBag.HayvanAltGruplari = _context.HayvanAltGruplari
                    .Where(x => x.GrupID == hayvan.HayGrupID)
                    .Select(x => new SelectListItem
                    {
                        Value = x.AltGrupID.ToString(),
                        Text = x.AltGrupAdi,
                        Selected = x.AltGrupID == hayvan.HayAltGrupID
                    }).ToList();

                ViewBag.HayvanTurleri = _context.HayvanTurleri
                    .Where(x => x.AltGrupID == hayvan.HayAltGrupID)
                    .Select(x => new SelectListItem
                    {
                        Value = x.TurID.ToString(),
                        Text = x.TurAdi,
                        Selected = x.TurID == hayvan.HayTurID
                    }).ToList();

                var ilceler = await _izmirBBService.GetIlcelerAsync();
                ViewBag.Hayvan = hayvan;

               
               if (!string.IsNullOrEmpty(hayvan.Ilce))
               {
                    var mahalleler = await _izmirBBService.GetMahallelerAsync(hayvan.Ilce.ToString());
                    ViewBag.Mahalleler = mahalleler;
                }

                return View(ilceler);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Veriler yüklenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(Hayvanlarim hayvan)
        {
            ModelState.Remove("HayvanTurleri");
            ModelState.Remove("HayvanAltGruplari");
            ModelState.Remove("HayvanTuruGruplari");
            ModelState.Remove("Users");
            hayvan.UserID = CurrentUserId;
            hayvan.KayitTarihi = DateTime.Now;
            ModelState.Remove("Durum");
            hayvan.Durum = "Aktif";
            hayvan.Il = "0";


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurunuz.";
                return RedirectToAction("Edit", new { id = hayvan.Id });
            }

            var existingHayvan = _context.Hayvanlarim
                .FirstOrDefault(h => h.Id == hayvan.Id && h.UserID == CurrentUserId);

            if (existingHayvan == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            existingHayvan.HayGrupID = hayvan.HayGrupID;
            existingHayvan.HayAltGrupID = hayvan.HayAltGrupID;
            existingHayvan.HayTurID = hayvan.HayTurID;
            existingHayvan.Ilce = hayvan.Ilce;
            existingHayvan.Mahalle = hayvan.Mahalle;
            existingHayvan.Adet = hayvan.Adet;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Kayıt başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}
