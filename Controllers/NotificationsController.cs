using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TarimHibe.Models;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public class NotificationsController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;
        private readonly ICbsService _cbsService;
        private readonly INotificationService _ns;

        private int CurrentUserId => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

        public NotificationsController(
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _ns.GetUserNotificationsAsync(CurrentUserId);
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _ns.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
