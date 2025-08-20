using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TarimHibe.Models;
using TarimHibe.Helpers;
using System.Linq;
using System.Threading.Tasks;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;

        public ProfileController(MenuService menuService, HibeDbContext context, IzmirBBService izmirBBService) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
        }

        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

        [HttpGet]
        public IActionResult Index()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(idStr, out var userId))
                return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                UserID = user.UserID,
                Ad = user.Ad,
                Soyad = user.Soyad,
                Telefon = user.Telefon,
                Email = user.Email,
                Il = user.Il,
                Ilce = user.Ilce,
                Mahalle = user.Mahalle
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel m)
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(idStr, out var userId))
                return RedirectToAction("Login", "Auth");

            // Load existing user to preserve profile fields on error
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            // If validation fails (e.g. password error), re-populate profile fields
            void PopulateProfileFields()
            {
                m.Ad = user.Ad;
                m.Soyad = user.Soyad;
                m.Telefon = user.Telefon;
                m.Email = user.Email;
                m.Il = user.Il;
                m.Ilce = user.Ilce;
                m.Mahalle = user.Mahalle;
            }

            // Model validations
            if (!ModelState.IsValid)
            {
                PopulateProfileFields();
                return View(m);
            }

            // Update profile info
            user.Ad = m.Ad;
            user.Soyad = m.Soyad;
            user.Telefon = m.Telefon;
            user.Email = m.Email;
            user.Il = m.Il;
            user.Ilce = m.Ilce;
            user.Mahalle = m.Mahalle;

            // Change password if new provided
            if (!string.IsNullOrEmpty(m.CurrentPassword) || !string.IsNullOrEmpty(m.NewPassword))
            {
                var hashedCurrent = SecurityHelper.HashPassword(m.CurrentPassword);
                if (user.Parola != hashedCurrent)
                {
                    ModelState.AddModelError(nameof(m.CurrentPassword), "Mevcut parola hatalı.");
                    PopulateProfileFields();
                    return View(m);
                }
                user.Parola = SecurityHelper.HashPassword(m.NewPassword);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profiliniz güncellendi.";
            return RedirectToAction("Index");
        }
    }
}