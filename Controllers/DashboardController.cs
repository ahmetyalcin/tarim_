using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Filters;
using TarimHibe.Models;
using TarimHibe.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TarimHibe.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;

        public DashboardController(
            MenuService menuService,
            HibeDbContext context,
            IzmirBBService izmirBBService
        ) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
        }

        // Admin Dashboard
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                ToplamKullanici = await _context.Users.CountAsync(),
                ToplamHibeBasvuru = await _context.HibeBasvurular.CountAsync(),
                ToplamArazi = await _context.Araziler.CountAsync(),
                ToplamHayvan = await _context.Hayvanlarim.CountAsync(),
                ToplamEkipman = await _context.Ekipmanlar.CountAsync(),

                Son10Ciftci = await _context.Users
                    .OrderByDescending(u => u.UserID)
                    .Take(10)
                    .Select(u => new AdminDashboardViewModel.CiftciItem
                    {
                        AdSoyad = u.Ad,
                        Email = u.Email,
                        KayitTarihi = Convert.ToDateTime(u.KayitTarihi)
                    })
                    .ToListAsync()
            };

            return View(vm);
        }


        [AuthorizeRole("User")]
        public async Task<IActionResult> UserIndex()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Auth");

            // Kullanıcı adını al
            var user = await _context.Users.FindAsync(userId);
            ViewBag.AdSoyad = user?.Ad ?? "Kullanıcı";

            // Mevcut veriler
            var hibeBasvuruSayisi = await _context.HibeBasvurular
                .CountAsync(b => b.UserID == userId);

            var araziSayisi = await _context.Araziler
                .CountAsync(a => a.UserID == userId);

            var hayvanSayisi = await _context.Hayvanlarim
                .CountAsync(h => h.UserID == userId);

            var ekipmanSayisi = await _context.Ekipmanlar
                .CountAsync(e => e.UserID == userId);

            // Açık hibeler
            var yeniHibeler = await _context.Hibeler
                .Where(h => h.Yayinda)
                .OrderByDescending(h => h.BaslangicTarihi)
                .Take(10)
                .Select(h => new DashboardViewModel.HibeMiniViewModel
                {
                    HibeID = h.HibeID,
                    HibeTuru = h.HibeTuru,
                    Miktar = h.Miktar,
                    UcretliMi = h.UcretliMi,
                    BaslangicTarihi = h.BaslangicTarihi,
                    BitisTarihi = h.BitisTarihi
                })
                .ToListAsync();

            // Bu yıla ait onaylı CKS var mı?
            var currentYear = DateTime.Now.Year;
            var hasCurrentCks = await _context.CKS_Bilgileri
                .AnyAsync(c => c.UserID == userId
                           && c.OnayDurumu == 1
                           && c.Yil == currentYear);

            // Bekleyen CKS var mı?
            var hasPendingCks = await _context.CKS_Bilgileri
                .AnyAsync(c => c.UserID == userId
                           && c.OnayDurumu == 0
                           && c.Yil == currentYear);

            // Trend hesaplamaları (geçen aya göre)
            var lastMonth = DateTime.Now.AddMonths(-1);
            var lastMonthHibeSayisi = await _context.HibeBasvurular
                .Where(h => h.UserID == userId && h.BasvuruTarihi < lastMonth)
                .CountAsync();

            var vm = new DashboardViewModel
            {
                HibeBasvuruSayisi = hibeBasvuruSayisi,
                AraziSayisi = araziSayisi,
                HayvanSayisi = hayvanSayisi,
                EkipmanSayisi = ekipmanSayisi,
                HasCurrentCks = hasCurrentCks,
                YeniHibeler = yeniHibeler,

                // Hero ayarları (varsayılan değerlerle - tablolar yoksa da çalışır)
                HeroTitle = "Tarım Hibeleri Yönetim Sistemi",
                HeroSubtitle = "Tarım, bu şehrin bereketi ve geleceğidir. Çiftçilerimizin her zaman yanında olmak için modern tarım teknolojilerini ve sürdürülebilir üretimi destekliyoruz. Hedefimiz; daha verimli topraklar, daha güçlü bir üretim ve kendi kendine yetebilen bir şehir",
                HeroImageUrl = "https://images.unsplash.com/photo-1574323347407-f5e1ad6d020b?w=400&h=300&fit=crop",
                WelcomeBadgeText = "İyi Tarım'a Hoş Geldiniz!",

                // Slide items - boş liste (tablolar yoksa da çalışır)
                SlideItems = new List<DashboardViewModel.SlideItemViewModel>
        {
            new DashboardViewModel.SlideItemViewModel
            {
                Id = 1,
                Title = "Büyükşehir Yapan Köylere Can Suyu Oluyor",
                Description = "Modern sulama sistemleri ile verimlilik artırılıyor",
                BackgroundColor = "#1e3c72",
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new DashboardViewModel.SlideItemViewModel
            {
                Id = 2,
                Title = "Başkan Tugay: Hedefimiz Tarım Üretiminde Yeni Bir Şehir Olmak",
                Description = "25 Eylül 2024 Pazartesi - Sürdürülebilir tarım projelerimiz devam ediyor",
                BackgroundColor = "#667eea",
                CreatedDate = DateTime.Now,
                IsActive = true
            }
        },

                // Trend hesaplamaları
                HibeBasvuruTrend = hibeBasvuruSayisi > lastMonthHibeSayisi ? "up" :
                                  hibeBasvuruSayisi < lastMonthHibeSayisi ? "down" : "stable",
                AraziTrend = "stable",
                HayvanTrend = "stable",
                EkipmanTrend = "stable"
            };

            // ViewBag'e ek bilgiler
            ViewBag.CurrentYear = currentYear;
            ViewBag.HasPendingThisYear = hasPendingCks;

            return View("UserIndex", vm);
        }




        // Admin için Hero ayarları yönetimi
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> HeroSettings()
        {
            var heroSettings = await _context.HeroSettings
                .OrderByDescending(h => h.CreatedDate)
                .ToListAsync();

            return View(heroSettings);
        }

        [HttpPost]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> CreateHeroSetting(HeroSetting model)
        {
            if (ModelState.IsValid)
            {
                // Önce tüm mevcut ayarları pasif yap
                var existingSettings = await _context.HeroSettings.ToListAsync();
                foreach (var setting in existingSettings)
                {
                    setting.IsActive = false;
                }

                // Yeni ayarı ekle
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                _context.HeroSettings.Add(model);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hero ayarları başarıyla oluşturuldu.";
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // Admin için Slide yönetimi
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> SlideItems()
        {
            var slideItems = await _context.SlideItems
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedDate)
                .ToListAsync();

            return View(slideItems);
        }

        [HttpPost]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> CreateSlideItem(SlideItem model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                _context.SlideItems.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Slide başarıyla oluşturuldu.";
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> ToggleSlideStatus(int id)
        {
            var slide = await _context.SlideItems.FindAsync(id);
            if (slide != null)
            {
                slide.IsActive = !slide.IsActive;
                slide.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, isActive = slide.IsActive });
            }

            return Json(new { success = false });
        }
    }
}