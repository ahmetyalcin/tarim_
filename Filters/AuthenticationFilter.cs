using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TarimHibe.Models;

namespace TarimHibe.Filters
{
    /// <summary>
    /// Global Login kontrolü. 
    /// /Auth/Login, /Auth/Register, /Auth/RecoverPw ve [AllowAnonymous] işaretlilerini atlar.
    /// </summary>
    public class AuthenticationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 1) Eğer [AllowAnonymous] varsa skip
            if (context.ActionDescriptor.EndpointMetadata
                        .OfType<AllowAnonymousAttribute>()
                        .Any())
            {
                return;
            }

            // 2) Belirli Auth URL’lerini atla
            var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
            if (path.StartsWith("/auth/login") ||
                path.StartsWith("/auth/register") ||
                path.StartsWith("/auth/recoverpw"))
            {
                return;
            }

            // 3) Session’da UserId yoksa Login’e yönlendir
            var userId = context.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "Login" }));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Boş bırakabilirsiniz
        }
    }

    /// <summary>
    /// Rol bazlı erişim kontrolü.
    /// - Parametresiz ctor: sadece Login kontrolü (globalde zaten yapıldı).
    /// - [AuthorizeRole("Admin","User")] ile spesifik roller kontrol edilir.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        // Global filtre eklemesinde çağrılan ctor: yalnızca login yeterli
        public AuthorizeRoleAttribute()
        {
            _allowedRoles = Array.Empty<string>();
        }

        // [AuthorizeRole("Admin","User")] kullanımı
        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 1) Eğer [AllowAnonymous] varsa skip
            if (context.ActionDescriptor.EndpointMetadata
                        .OfType<AllowAnonymousAttribute>()
                        .Any())
            {
                return;
            }

            // 2) Session’da UserId yoksa Login’e yönlendir
            var idStr = context.HttpContext.Session.GetString("UserId");
            if (!int.TryParse(idStr, out var userId))
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "Login" }));
                return;
            }

            // 3) Parametresiz ctor kullanıldıysa (global ekleme), login yeterli
            if (_allowedRoles.Length == 0)
                return;
            
            // 4) Rol kontrolü
            var db = context.HttpContext.RequestServices.GetRequiredService<HibeDbContext>();
            var userRole = db.Users
                             .Where(u => u.UserID == userId)
                             .Select(u => u.Role.RoleName)
                             .FirstOrDefault();

            if (string.IsNullOrEmpty(userRole) ||
                !_allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                // Kullanıcının yetkisi yok Dashboard’a geri yolla
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));

            }

        }
    }
}
