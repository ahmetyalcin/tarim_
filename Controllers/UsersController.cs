using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;
using TarimHibe.Services;
using System.Linq.Dynamic.Core; // NuGet: System.Linq.Dynamic.Core

namespace TarimHibe.Controllers
{
    public class UsersController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;

        public UsersController(MenuService menuService, HibeDbContext context, IzmirBBService izmirBBService) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
        }

        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

        public async Task<IActionResult> Index()
        {
            // Sadece boş view döndür, veriler AJAX ile yüklenecek
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var draw = int.Parse(Request.Form["draw"].FirstOrDefault() ?? "0");
                var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
                var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "UserID";
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";

                // Özel filtreler
                var roleFilter = Request.Form["roleFilter"].FirstOrDefault() ?? "";

                // Base query
                var query = _context.Users
                    .Include(u => u.Role)
                    .AsQueryable();

                // Arama filtresi
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(u =>
                        u.TcKimlikNo.Contains(searchValue) ||
                        u.Ad.Contains(searchValue) ||
                        u.Soyad.Contains(searchValue) ||
                        u.Telefon.Contains(searchValue) ||
                        u.Email.Contains(searchValue) ||
                        u.Role.RoleName.Contains(searchValue)
                    );
                }

                // Rol filtresi
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query = query.Where(u => u.Role.RoleName == roleFilter);
                }

                // Toplam kayıt sayısı
                var totalRecords = await _context.Users.CountAsync();
                var filteredRecords = await query.CountAsync();

                // Sıralama
                var orderBy = $"{sortColumn} {sortDirection}";
                switch (sortColumn.ToLower())
                {
                    case "tcKimlik":
                        orderBy = $"TcKimlikNo {sortDirection}";
                        break;
                    case "adSoyad":
                        orderBy = $"Ad {sortDirection}";
                        break;
                    case "telefon":
                        orderBy = $"Telefon {sortDirection}";
                        break;
                    case "email":
                        orderBy = $"Email {sortDirection}";
                        break;
                    case "roleName":
                        orderBy = $"Role.RoleName {sortDirection}";
                        break;
                    default:
                        orderBy = $"UserID {sortDirection}";
                        break;
                }

                // Sayfalama ve veri çekme
                var users = await query
                    .OrderBy(orderBy)
                    .Skip(start)
                    .Take(length)
                    .Select(u => new UserViewModel
                    {
                        UserID = u.UserID,
                        TcKimlik = u.TcKimlikNo,
                        AdSoyad = u.Ad + " " + u.Soyad,
                        Telefon = u.Telefon,
                        Email = u.Email,
                        RoleName = u.Role.RoleName
                    })
                    .ToListAsync();

                // DataTables response formatı
                var response = new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = users
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRole(int id, string toRole)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == toRole);
                if (role == null)
                    return Json(new { success = false, message = "Rol bulunamadı: " + toRole });

                user.RoleId = role.RoleId;
                await _context.SaveChangesAsync();

                return Json(new { success = true, newRole = toRole });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
    }
}