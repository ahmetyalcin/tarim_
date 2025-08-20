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

namespace TarimHibe.Controllers
{
    public class AraziController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;
        private readonly ICbsService _cbsService;

        public AraziController(
            MenuService menuService,
            HibeDbContext context,
            IzmirBBService izmirBBService,
            ICbsService cbsService
        ) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
            _cbsService = cbsService;
        }

        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");


        public async Task<IActionResult> Index()
        {


            var araziler1 = await _context.Araziler
            .Include(a => a.CKSBelgeleri)
            .Where(a => a.UserID == CurrentUserId)
            .ToListAsync();


            var araziler = await _context.Araziler
            .Include(a => a.MulkiyetBilgisi)
            .Include(a => a.AraziTuruBilgisi)
            .Include(a => a.SulamaBilgisi)
            .Include(a => a.CKSBelgeleri)
            .Where(a => a.UserID == CurrentUserId)
            .ToListAsync();

            return View(araziler);
        }

        // Add sayfası açıldığında ilçe listesini de gönderiyoruz
        [HttpGet]
        public async Task<IActionResult> Add()
        {

            ViewBag.AraziMulkiyet = _context.AraziMulkiyet
    .Select(x => new SelectListItem { Value = x.MulkiyetId.ToString(), Text = x.MulkiyetAdi }).ToList();

            ViewBag.AraziTurleri = _context.AraziTurleri
.Select(x => new SelectListItem { Value = x.AraziTurId.ToString(), Text = x.AraziTurAdi }).ToList();


            ViewBag.Arazisulama = _context.AraziSulama
.Select(x => new SelectListItem { Value = x.AraziSulamaId.ToString(), Text = x.AraziSulamaAdi }).ToList();

            ViewBag.AraziKullanim = _context.AraziKullanim
.Select(x => new SelectListItem { Value = x.AraziKullanimId.ToString(), Text = x.AraziKullanimAdi }).ToList();

            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID.ToString() == userId);
                if (user != null)
                {
                    // Doğum tarihini uygun formatta ViewBag'e gönder
                    ViewBag.DogumTarihi = user.DogumTarihi.ToString("yyyy-MM-dd");
                }
            }

            var ilceler = await _cbsService.GetIlcelerAsync();
            return View(ilceler);
        }

        [HttpGet]
        public async Task<IActionResult> IlceGetir()
        {
            try
            {
                var ilceler = await _cbsService.GetIlcelerAsync();
                return Json(ilceler);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IlceGetir error: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MahalleGetir(string ilceId)
        {
            try
            {
                var mahalle = await _cbsService.GetMahallelerAsync(ilceId);
                return Json(mahalle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MahalleGetir error: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        //[HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AraziSorgula(
            string ilce, string mahalle, string ada, string parsel)
        {
            try
            {


                // 1. Daha önce kayıtlı mı kontrol et
                var kayitVarMi = await _context.Araziler.AnyAsync(a =>
                    a.Ilce == ilce &&
                    a.Mahalle == mahalle &&
                    a.Ada == ada &&
                    a.Parsel == parsel);

                if (kayitVarMi)
                {
                    Debug.WriteLine("Kayıt zaten var.");
                    return Json(new { error = "Bu ada-parsel-mahalle kombinasyonu zaten kayıtlıdır." });
                }
                  


                var parcelObj = await _cbsService.GetParselAsync(mahalle, ada, parsel);
                var wkt = parcelObj["geom"]?.Value<string>();
                if (string.IsNullOrEmpty(wkt))
                    return Json(new { error = "Parsel geometrisi yok." });

                // "POLYGON ((...))" kırp, sadece "x y, x y, ..." kalsın:
                wkt = wkt
                    .Replace("POLYGON ((", "")
                    .Replace("))", "")
                    .Trim();

                // split & parse
                var coords = wkt
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(pt => pt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(arr => arr.Length == 2)
                    .Select(arr =>
                    {
                        // arr[0]=lng, arr[1]=lat  (4326 ise)
                        var lng = double.Parse(arr[0], CultureInfo.InvariantCulture);
                        var lat = double.Parse(arr[1], CultureInfo.InvariantCulture);
                        return new[] { lng, lat };
                    })
                    .ToList();

                if (coords.Count < 3)
                    return Json(new { error = "Parsel koordinatları eksik." });

                // kapalı poligon için başı sona ekle
                coords.Add(coords[0]);

                return Json(new { success = true, data = coords });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AraziSorgula error: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Arazi model, IFormFile CksBelgesi)
        {
            model.UserID = CurrentUserId;
            model.KayitTarihi = DateTime.Now;
            model.IlceAdi = Request.Form["IlceAdi"];
            model.MahalleAdi = Request.Form["MahalleAdi"];
            _context.Araziler.Add(model);
            await _context.SaveChangesAsync();

            //string filePath = string.Empty;
            //var fileName = $"{Guid.NewGuid()}_{CksBelgesi.FileName}";
            //if (CksBelgesi != null && CksBelgesi.Length > 0)
            //{
               
            //    filePath = Path.Combine("wwwroot/uploads/cks", fileName);
            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await CksBelgesi.CopyToAsync(stream);
            //    }
            //}

            // CKS_Bilgileri tablosuna kayıt
            /*    OnaySurecinde = 0,
                  Onaylandi = 1,
                  Onaylanmadi = 2
            */
            var cks = new CKS_Bilgileri
            {
                AraziId =0,  // SaveChanges sonrası model.Id atanmış olur
              //  TcKimlikNo = Request.Form["TCNo"],
              //  DogumTarihi = DateTime.Parse(Request.Form["DogumTarihi"]),
                Yil = int.Parse(Request.Form["UretimYili"]),
                BelgeDosyasi = "0",
                UserID = CurrentUserId,
                OnayDurumu = 0,
                KayitTarihi = DateTime.Now
            };
            
          //  _context.CKS_Bilgileri.Add(cks);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> AraziKoordinatGetir([FromForm] string ilce, [FromForm] string mahalle, [FromForm] string ada, [FromForm] string parsel)
        {
            try
            {
                Console.WriteLine($"→ Edit sorgu: {ilce} | {mahalle} | {ada} | {parsel}");

                var parcelObj = await _cbsService.GetParselAsync(mahalle, ada, parsel);
                var wkt = parcelObj["geom"]?.Value<string>();
                if (string.IsNullOrEmpty(wkt))
                    return Json(new { success = false, error = "Parsel geometrisi yok." });

                // POLYGON ((x y, x y...)) → kırp
                wkt = wkt
                    .Replace("POLYGON ((", "")
                    .Replace("))", "")
                    .Trim();

                var coords = wkt
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(pt => pt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(arr => arr.Length == 2)
                    .Select(arr =>
                    {
                        var lng = double.Parse(arr[0], CultureInfo.InvariantCulture);
                        var lat = double.Parse(arr[1], CultureInfo.InvariantCulture);
                        return new[] { lng, lat };
                    })
                    .ToList();

                if (coords.Count < 3)
                    return Json(new { success = false, error = "Yetersiz koordinat verisi." });

                coords.Add(coords[0]); // kapalı poligon için

                return Json(new { success = true, data = coords });
            }
            catch (Exception ex)
            {
                Console.WriteLine("AraziKoordinatGetir ERROR → " + ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var arazi = await _context.Araziler
                .Include(a => a.MulkiyetBilgisi)
                .Include(a => a.AraziTuruBilgisi)
                .Include(a => a.SulamaBilgisi)
                 .Include(a => a.CKSBelgeleri)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (arazi == null)
                return NotFound();

            ViewBag.AraziMulkiyet = _context.AraziMulkiyet
                .Select(x => new SelectListItem { Value = x.MulkiyetId.ToString(), Text = x.MulkiyetAdi })
                .ToList();

            ViewBag.AraziTurleri = _context.AraziTurleri
                .Select(x => new SelectListItem { Value = x.AraziTurId.ToString(), Text = x.AraziTurAdi })
                .ToList();

            ViewBag.Arazisulama = _context.AraziSulama
                .Select(x => new SelectListItem { Value = x.AraziSulamaId.ToString(), Text = x.AraziSulamaAdi })
                .ToList();

            ViewBag.AraziKullanim = _context.AraziKullanim
                .Select(x => new SelectListItem { Value = x.AraziKullanimId.ToString(), Text = x.AraziKullanimAdi })
                .ToList();

            ViewBag.IlceAdi = arazi.IlceAdi; // ya da ilce tablosundan ilce adı
            ViewBag.MahalleAdi = arazi.MahalleAdi; // ya da mahalle tablosundan mahalle adı


            return View(arazi);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, Arazi model)
        //{


        //    var arazi = await _context.Araziler.FindAsync(id);
        //    if (arazi == null)
        //        return NotFound();

        //    // Güncelle
        //    arazi.MulkiyetId = model.MulkiyetId;
        //    arazi.AraziTurId = model.AraziTurId;
        //    arazi.AraziSulamaId = model.AraziSulamaId;
        //    arazi.AraziKullanimId = model.AraziKullanimId;
        //    arazi.Mevkii = model.Mevkii;
        //    arazi.ToplamAlani = model.ToplamAlani;
        //    arazi.HisseAlani = model.HisseAlani;
        //    // İstersen KayitTarihi = DateTime.Now gibi log da yapabilirsin

        //    _context.Update(arazi);
        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = "Arazi bilgileri güncellendi.";
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Arazi model, IFormFile CksBelgesi)
        {
            var arazi = await _context.Araziler
                .Include(a => a.CKSBelgeleri)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (arazi == null)
                return NotFound();

            // Güncelle
            arazi.MulkiyetId = model.MulkiyetId;
            arazi.AraziTurId = model.AraziTurId;
            arazi.AraziSulamaId = model.AraziSulamaId;
            arazi.AraziKullanimId = model.AraziKullanimId;
            arazi.Mevkii = model.Mevkii;
            arazi.ToplamAlani = model.ToplamAlani;
            arazi.HisseAlani = model.HisseAlani;

            _context.Update(arazi);

            // ÇKS Belgesi Güncelleme
            var mevcutCks = arazi.CKSBelgeleri.FirstOrDefault();

            if (mevcutCks != null && mevcutCks.OnayDurumu == 2) // Sadece onaylanmadıysa yeni yüklemeye izin var
            {
                if (CksBelgesi != null && CksBelgesi.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{CksBelgesi.FileName}";
                    var path = Path.Combine("wwwroot/uploads/cks", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await CksBelgesi.CopyToAsync(stream);
                    }

                    // Yeni belgeyle güncelle ve onay sürecine al
                    mevcutCks.BelgeDosyasi = fileName;
                    mevcutCks.OnayDurumu = 0; // tekrar onay sürecine girsin
                    mevcutCks.KayitTarihi = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Arazi bilgileri güncellendi.";
            return RedirectToAction("Index");
        }






    }
}
