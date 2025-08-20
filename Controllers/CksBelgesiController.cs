using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
          // DbContext buradaysa
using TarimHibe.Models;         // CKS_Bilgileri modeli buradaysa
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TarimHibe.Controllers;
using TarimHibe.Services;

public class CksBelgesiController : BaseController
{
    private readonly HibeDbContext _context;
    public CksBelgesiController(MenuService menuSvc, HibeDbContext ctx)
        : base(menuSvc)
    {
        _context = ctx;
    }

    private int CurrentUserIntId
        => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

    // GET: /CksBelgesi
    public async Task<IActionResult> Index()
    {
        // Bu yılın onaylı belgesini alıyoruz:
        var currentYear = DateTime.Now.Year;
        var docs = await _context.CKS_Bilgileri
          .Where(c => c.UserID == CurrentUserIntId) // sadece kullanıcıya ait
          .OrderByDescending(c => c.KayitTarihi)
          .ToListAsync();

        // ViewModel veya ViewBag’le bu yıl onaylıyı ayrıca işaretleyebilirsiniz:
        ViewBag.CurrentYear = currentYear;
        ViewBag.HasValidThisYear = docs.Any(c => c.Yil == currentYear && c.OnayDurumu == 1);

        return View(docs);
    }

    [HttpPost, ValidateAntiForgeryToken]

    public async Task<IActionResult> Upload(IFormFile file, int yil)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
            return RedirectToAction(nameof(Index));
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine("wwwroot/uploads/cks", fileName);
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        var entity = new CKS_Bilgileri
        {
            UserID = CurrentUserIntId,
            AraziId = null,
            BelgeDosyasi = fileName,
            Yil = yil, 
            OnayDurumu = 0,
            KayitTarihi = DateTime.Now
        };

        _context.CKS_Bilgileri.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "CKS belgeniz yüklendi.";
        return RedirectToAction(nameof(Index));
    }

}
