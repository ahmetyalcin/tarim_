using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TarimHibe.Models;
using TarimHibe.Models.ApiModels;
using TarimHibe.Services;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using TarimHibe.Filters;

namespace TarimHibe.Controllers
{
    public class CksOnayController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;
        private readonly ICbsService _cbsService;
        private readonly INotificationService _ns;


        public CksOnayController(
            MenuService menuService,
            HibeDbContext context,
            IzmirBBService izmirBBService,
            ICbsService cbsService,
            INotificationService notificationService
        ) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
            _cbsService = cbsService;
            _ns = notificationService;
        }
        // GET: /CksOnay?yil=2024&durum=1
        [HttpGet]
        public async Task<IActionResult> Index(int? yil, int? durum)
        {
            // 1) Filtrelenmemiş sorgu
            var query = _context.CKS_Bilgileri
                        .Include(c => c.Arazi)
                        .Include(c => c.User)
                        .AsQueryable();

            // 2) Yıla göre
            if (yil.HasValue)
                query = query.Where(c => c.Yil == yil.Value);

            // 3) Duruma göre
            if (durum.HasValue)
                query = query.Where(c => c.OnayDurumu == durum.Value);

            // 4) ViewBag için dropdown listeleri
            var yillar = await _context.CKS_Bilgileri
                                .Select(c => c.Yil)
                                .Distinct()
                                .OrderByDescending(x => x)
                                .ToListAsync();
            ViewBag.Yillar = new SelectList(yillar, yil);

            var durumlar = new[]
            {
                new { Value = 0, Text = "Beklemede" },
                new { Value = 1, Text = "Onaylandı"  },
                new { Value = 2, Text = "Reddedildi" }
            };
            ViewBag.Durumlar = new SelectList(durumlar, "Value", "Text", durum);

            // 5) Sonuçları VM’e dönüştür
            var model = await query
                .OrderByDescending(c => c.CKSID)
                .Select(c => new CksOnayViewModel
                {
                    CksID = c.CKSID,
                    Yil = c.Yil,
                    Kullanici = c.User.Ad + " " + c.User.Soyad,
                    AraziBilgi = $"{c.Arazi.IlceAdi} (Ada {c.Arazi.Ada}/P {c.Arazi.Parsel})",
                    BelgeYolu = c.BelgeDosyasi ?? "",
                    OnayDurumu = c.OnayDurumu
                })
                .ToListAsync();

            return View(model);
        }

        // 6) Onayla
        [HttpPost, AuthorizeRole("Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var rec = await _context.CKS_Bilgileri.FindAsync(id);
            if (rec != null)
            {
                rec.OnayDurumu = 1;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "CKS belgesi onaylandı.";
            }
            return RedirectToAction(nameof(Index));
        }

        // 7) Reddet
        [HttpPost, AuthorizeRole("Admin")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var rec = await _context.CKS_Bilgileri.FindAsync(id);
            if (rec != null)
            {
                rec.OnayDurumu = 2;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "CKS belgesi reddedildi.";

                await _ns.SendAsync(
                   rec.UserID,
                   "ÇKS Belgeniz Reddedildi",
                   string.IsNullOrWhiteSpace(reason)
                       ? "Gerekli evraklar eksikti."
                       : $"ÇKS belgeniz reddedildi: {reason}"
               );


            }
            return RedirectToAction(nameof(Index));
        }
    }
}