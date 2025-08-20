using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using TarimHibe.Models;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly MenuService _menuService;

        public BaseController(MenuService menuService)
        {
            _menuService = menuService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var roleName = context.HttpContext.Session.GetString("Role") ?? "User";

            List<MenuItem> menu = _menuService
               .GetMenuForRoleAsync(roleName)
               .GetAwaiter()
               .GetResult();

            ViewBag.SidebarMenu = menu;
            base.OnActionExecuting(context);

        }
    }
}