using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public class HibelerimController : BaseController
    {
        private readonly HibeDbContext _context;
        public HibelerimController(MenuService menuService, HibeDbContext context)
            : base(menuService)
        {
            _context = context;
        }

        // Oturumdan user id çekiyoruz
        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

        public async Task<IActionResult> Index()
        {

            var list = await _context.HibeBasvurular
                .Where(b => b.UserID == CurrentUserId)
                .Include(b => b.Hibe)
                .Include(b => b.Arazi)
                .OrderByDescending(b => b.BasvuruTarihi)
                .Select(b => new BasvuruViewModel
                {
                    BasvuruID = b.BasvuruID,
                    HibeTuru = b.Hibe.HibeTuru,
                    AraziBilgi = $"{b.Arazi.IlceAdi +" "+ b.Arazi.MahalleAdi} (Ada {b.Arazi.Ada}/Parsel {b.Arazi.Parsel})",
                    BasvuruTarihi = b.BasvuruTarihi,
                    AdminOnayDurumu = b.AdminOnayDurumu,
                    BasvuruDurumu = b.BasvuruDurumu 
                })
                .ToListAsync();
            return View(list);
        }
    }
}
