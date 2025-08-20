// Services/MenuService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;

namespace TarimHibe.Services
{
    public class MenuService
    {
        private readonly HibeDbContext _db;

        public MenuService(HibeDbContext db)
        {
            _db = db;
        }

        public async Task<List<MenuItem>> GetMenuForUserAsync(int userId)
        {
            var allowedIds = await _db.UserMenus
                .Where(um => um.UserId == userId)
                .Select(um => um.MenuItemId)
                .ToListAsync();
            if (!allowedIds.Any())
                return new List<MenuItem>();

            var idList = string.Join(",", allowedIds);
            var sql = $@"
        SELECT
            Id,
            Name,
            ISNULL(Icon, '')    AS Icon,
            HasSubMenu,
            TopMenuId,
            ISNULL(Link, '')    AS Link,
            [Index],
            EntityStatus
        FROM MenuItems
        WHERE Id IN ({idList})
        ORDER BY [Index];
    ";

            // ← Burada DbSet<MenuItem> kullanın
            var items = await _db.MenuItems
                .FromSqlRaw(sql)
                .ToListAsync();

            // Tree yapısı için yine TopMenuId kullanın
            var lookup = items.ToDictionary(i => i.Id);
            foreach (var item in items)
            {
                if (item.TopMenuId.HasValue &&
                    lookup.TryGetValue(item.TopMenuId.Value, out var parent))
                {
                    parent.Children.Add(item);
                }
            }

            return items.Where(i => i.TopMenuId == null).ToList();
        }

        /*
        public async Task<List<MenuItem>> GetMenuForRoleAsync(string roleName)
        {
            // role id al
            var role = await _db.Roles
                .Where(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            // rolde izinli menüleri al kontrol et ahmet
            var flat = await _db.MenuRoles
                .Where(mr => mr.RoleId == role)
                .Select(mr => mr.MenuItem)
                .Distinct()
                .ToListAsync();

            // Tree yapısını oluştur
            var lookup = flat.ToDictionary(m => m.Id);
            foreach (var m in flat)
            {
                if (m.TopMenuId.HasValue && lookup.TryGetValue(m.TopMenuId.Value, out var parent))
                    parent.Children.Add(m);
            }
            return flat.Where(m => m.TopMenuId == null).OrderBy(m => m.Index).ToList();
        }
        */

        public async Task<List<MenuItem>> GetMenuForRoleAsync(string roleName)
        {
            // 1) Fetch the RoleId for that roleName (case-insensitive)
            //    We rely on SQL Server’s default case-insensitive collation, 
            //    so simple == will do.  If you’re on a case-sensitive collation,
            //    switch to .ToUpper() == roleName.ToUpper() instead.
            var roleId = await _db.Roles
                .Where(r => r.RoleName == roleName)
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            if (roleId == 0)
                return new List<MenuItem>();

            // 2) Grab all MenuItems that this role is allowed to see
            var flat = await _db.MenuRoles
                .Where(mr => mr.RoleId == roleId)
                .Select(mr => mr.MenuItem)
                .Distinct()
                .ToListAsync();

            // 3) Build up the parent→child tree in memory
            var lookup = flat.ToDictionary(m => m.Id);
            foreach (var item in flat)
            {
                if (item.TopMenuId.HasValue
                 && lookup.TryGetValue(item.TopMenuId.Value, out var parent))
                {
                    parent.Children.Add(item);
                }
            }

            // 4) Return only the top-level items, ordered by your Index
            return flat
              .Where(m => m.TopMenuId == null)
              .OrderBy(m => m.Index)
              .ToList();
        }



    }
}
