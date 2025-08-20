using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;
using TarimHibe.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TarimHibe.Filters;

namespace TarimHibe.Controllers
{
    public class HibeBasvurularController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly ICbsService _cbsService;

        private readonly INotificationService _ns;

        public HibeBasvurularController(MenuService menuService, HibeDbContext context, ICbsService cbsService, INotificationService notificationService)
            : base(menuService)
        {
            _context = context;
            _cbsService = cbsService;
            _ns = notificationService;
        }

        private int CurrentUserIntId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

     private string CurrentUserId => HttpContext.Session.GetString("UserId") ?? "";
        // --- 1. Hibeler liste sayfası ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var hasArazi = await _context.Araziler
    .AnyAsync(a => a.UserID == CurrentUserIntId);

            var hasApprovedCks = await _context.CKS_Bilgileri
                .AnyAsync(c => c.UserID == CurrentUserIntId && c.OnayDurumu == 1 && c.Yil == DateTime.Now.Year);


            // 1. Daha önce hangi hibelere başvuru yapılmış
            var myAppliedHibeIds = await _context.HibeBasvurular
          .Where(b => b.UserID == CurrentUserIntId
                      && b.BasvuruDurumu != "Reddedildi")
          .Select(b => b.HibeID)
          .Distinct()
          .ToListAsync();

            // 2. Kullanıcının onaylı CKS belgesi var mı
            //var hasApprovedCks = await _context.Araziler
            //    .Include(a => a.CKSBelgeleri)
            //    .Where(a => a.UserID == CurrentUserIntId)
            //    .AnyAsync(a => a.CKSBelgeleri.Any(c => c.OnayDurumu == 1));

            // 3. Hibeleri al
            var hibelerRaw = await _context.Hibeler
                .OrderByDescending(h => h.BaslangicTarihi)
                .ToListAsync();

            // 4. Bellekte ViewModel'e dönüştür
            var hibeler = hibelerRaw.Select(h => new HibeCardViewModel
            {
                HibeID = h.HibeID,
                HibeTuru = h.HibeTuru,
                Miktar = h.Miktar,
                UcretliMi = h.UcretliMi,
                Yayinda = h.Yayinda,
                HibeAciklama = h.HibeAciklama,
                HasApprovedCks = hasApprovedCks, // burada tanımlanmış olacak
                AlreadyApplied = myAppliedHibeIds.Contains(h.HibeID),
                HasRegisteredArazi = hasArazi
                
            }).ToList();

            return View(hibeler);
        }



        [HttpGet]
        public async Task<IActionResult> Apply(int id /* hibeID */)
        {
            // 1. Hibe var mı?
            var hibe = await _context.Hibeler.FindAsync(id);
            if (hibe == null)
                return NotFound();

            // 2. Kullanıcının bu yıl için onaylı CKS belgesi var mı?
            var hasApprovedCks = await _context.CKS_Bilgileri
                .AnyAsync(c => c.UserID == CurrentUserIntId && c.OnayDurumu == 1 && c.Yil == DateTime.Now.Year);

            // 3. Kullanıcının arazileri var mı?
            var araziler = await _context.Araziler
                .Where(a => a.UserID == CurrentUserIntId)
                .ToListAsync();

            // 4. İkisi de yoksa form açma
            if (!hasApprovedCks || !araziler.Any())
            {
                TempData["ErrorMessage"] = "Başvuru yapabilmek için onaylı ÇKS belgesi ve en az bir araziniz olmalı.";
                return RedirectToAction(nameof(Index));
            }

            // 5. Daha önce başvuru yapılmış mı (ve reddedilmemiş mi)?
            var oncekiBasvuru = await _context.HibeBasvurular
                .Where(b => b.HibeID == id && b.UserID == CurrentUserIntId)
                .OrderByDescending(b => b.BasvuruTarihi)
                .FirstOrDefaultAsync();

            if (oncekiBasvuru != null && oncekiBasvuru.BasvuruDurumu != "Reddedildi")
            {
                TempData["ErrorMessage"] = "Bu hibeye zaten başvuru yaptınız.";
                return RedirectToAction(nameof(Index));
            }

            // 6. ViewModel’e ata
            var vm = new HibeApplyViewModel
            {
                HibeID = hibe.HibeID,
                HibeTuru = hibe.HibeTuru,
                Miktar = hibe.Miktar,
                UcretliMi = hibe.UcretliMi,
                HibeAciklama = hibe.HibeAciklama,
                AraziList = araziler.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.IlceAdi}/{a.MahalleAdi} Ada:{a.Ada} Parsel:{a.Parsel}"
                }).ToList()
            };

            return View(vm);
        }




        // --- 3. Başvuruyu kaydet ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(HibeApplyForm form)
        {
            // 3.1 Aynı hibe kontrolü (reddedilenleri sayma)
            if (await _context.HibeBasvurular.AnyAsync(b =>
                    b.HibeID == form.HibeID
                    && b.UserID == CurrentUserIntId
                    && b.AraziID == form.AraziID
                    && b.BasvuruDurumu != "Reddedildi"    // <-- sadece reddedilmemişleri engelle
                ))
            {
                ModelState.AddModelError("", "Bu araziyle bu hibeye zaten başvuru yaptınız.");
            }

            // Eğer ücretli ise dekont şart
            var hibe = await _context.Hibeler.FindAsync(form.HibeID);
            if (hibe == null) ModelState.AddModelError("", "Hibe bulunamadı.");
            if (hibe != null && hibe.UcretliMi && form.DekontFile == null)
            {
                ModelState.AddModelError("DekontFile", "Ücretli hibelerde dekont yüklemek zorunludur.");
            }

            if (!ModelState.IsValid)
            {
                return await Apply(form.HibeID);
            }

            // 3.3 Yeni Başvuru nesnesi
            var basvuru = new HibeBasvuru
            {
                HibeID = form.HibeID,
                UserID = CurrentUserIntId,
                AraziID = form.AraziID,
                BasvuruTarihi = DateTime.Now,
                KayitTarihi = DateTime.Now
            };

            // 3.4 Dekont dosyasını kaydet (varsa)
            if (form.DekontFile != null)
            {
                var fileName = $"{Guid.NewGuid()}_{form.DekontFile.FileName}";
                var path = Path.Combine("wwwroot/uploads/dekont", fileName);
                using var st = new FileStream(path, FileMode.Create);
                await form.DekontFile.CopyToAsync(st);
                basvuru.Dekont = fileName;
                basvuru.BasvuruDurumu = "Dekont Bekleniyor";
            }
            
            _context.HibeBasvurular.Add(basvuru);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Başvurunuz alındı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> HibeAdminIndex(int? hibeId)
        {
            // 1. Base query
            var query = _context.HibeBasvurular
                .Include(b => b.Hibe)
                .Include(b => b.User)
                .Include(b => b.Arazi)
                .AsQueryable();

            // ** SADECE ÜCRETSİZLER **
            query = query.Where(b => b.Hibe.UcretliMi == false);

            // 2. HibeId filtre (isteğe bağlı)
            if (hibeId.HasValue)
                query = query.Where(b => b.HibeID == hibeId.Value);

            // 3. Projeksiyon
            var model = await query
                .OrderByDescending(b => b.BasvuruTarihi)
                .Select(b => new AdminHibeBasvuruViewModel
                {
                    BasvuruID = b.BasvuruID,
                    HibeTuru = b.Hibe.HibeTuru,
                    UcretliMi = b.Hibe.UcretliMi,
                    BasvuranAdi = b.User.Ad + " " + b.User.Soyad,
                    AraziBilgi = $"{b.Arazi.IlceAdi} (Ada {b.Arazi.Ada}/P {b.Arazi.Parsel})",
                    BasvuruTarihi = b.BasvuruTarihi,
                    DekontYolu = b.Dekont,
                    AdminOnay = b.AdminOnayDurumu,
                    BasvuruDurumu = b.BasvuruDurumu
                })
                .ToListAsync();

            // Hibe dropdown’u (isterseniz kaldırabilirsiniz)
            var hibeler = await _context.Hibeler
                .Where(h => !h.UcretliMi)
                .OrderBy(h => h.HibeTuru)
                .ToListAsync();

            ViewBag.HibeList = hibeler
                .Select(h => new SelectListItem
                {
                    Value = h.HibeID.ToString(),
                    Text = h.HibeTuru
                })
                .ToList();
            ViewBag.SelectedHibe = hibeId?.ToString() ?? "";

            return View(model);
        }


        // 2. Onayla
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var basvuru = await _context.HibeBasvurular.FindAsync(id);
            if (basvuru != null)
            {
                basvuru.AdminOnayDurumu = true;
                basvuru.BasvuruDurumu = "Onaylandı";
                await _context.SaveChangesAsync();

                // Bildirim gönder
                await _ns.SendAsync(
                    basvuru.UserID,
                    "Hibe Başvurunuz Onaylandı",
                    "Tebrikler! Başvurunuz başarıyla onaylandı."
                );

            }
            return RedirectToAction(nameof(HibeAdminIndex));
        }

        // 3. Reddet
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var basvuru = await _context.HibeBasvurular.FindAsync(id);
            if (basvuru != null)
            {
                basvuru.AdminOnayDurumu = false;
                basvuru.BasvuruDurumu = "Reddedildi";
                await _context.SaveChangesAsync();

                // Bildirim gönder
                await _ns.SendAsync(
               basvuru.UserID,
               "Hibe Başvurunuz Reddedildi",
               string.IsNullOrWhiteSpace(reason)
                   ? "Başvurunuz uygun görülmedi."
                   : $"Başvurunuz reddedildi: {reason}"
           );
            }
            return RedirectToAction(nameof(HibeAdminIndex));
        }


        [HttpGet]
        public async Task<IActionResult> AdminAllSubmissions(int? hibeId, string durum = "all")
        {
            // 1. Get base query of submissions
            var query = _context.HibeBasvurular
                .Include(b => b.Hibe)
                .Include(b => b.User)
                .Include(b => b.Arazi)
                .AsQueryable();

            // ** Sadece ücretli hibeleri al **
            query = query.Where(b => b.Hibe.UcretliMi);

            // 2. Apply hibeId filter (if any)
            if (hibeId.HasValue)
                query = query.Where(b => b.HibeID == hibeId.Value);

            // 3. Apply durum filter
            if (!string.Equals(durum, "all", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.BasvuruDurumu == durum);

            // 4. Project to your view‐model
            var model = await query
                .OrderByDescending(b => b.BasvuruTarihi)
                .Select(b => new AdminHibeBasvuruViewModel
                {
                    BasvuruID = b.BasvuruID,
                    HibeTuru = b.Hibe.HibeTuru,
                    UcretliMi = b.Hibe.UcretliMi,
                    BasvuranAdi = b.User.Ad + " " + b.User.Soyad,
                    AraziBilgi = $"{b.Arazi.IlceAdi} (Ada {b.Arazi.Ada}/P {b.Arazi.Parsel})",
                    BasvuruTarihi = b.BasvuruTarihi,
                    DekontYolu = b.Dekont,
                    BasvuruDurumu = b.BasvuruDurumu,
                    AdminOnay = b.AdminOnayDurumu
                })
                .ToListAsync();

            // 5. Build the Hibe dropdown
            var allHibeler = await _context.Hibeler
                .Where(h => h.UcretliMi)              // isterseniz burada da sadece ücretlileri listeleyebilirsiniz
                .OrderBy(h => h.HibeTuru)
                .ToListAsync();

            ViewBag.HibeList = new SelectList(allHibeler, nameof(Hibe.HibeID), nameof(Hibe.HibeTuru), hibeId?.ToString());
            ViewBag.SelectedDurum = durum;

            return View(model);
        }





    }


}

  

