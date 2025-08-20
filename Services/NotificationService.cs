using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TarimHibe.Models;
using Microsoft.EntityFrameworkCore;

namespace TarimHibe.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HibeDbContext _ctx;
        public NotificationService(HibeDbContext ctx)
            => _ctx = ctx;

        public async Task SendAsync(int userId, string title, string message)
        {
            var n = new Notification
            {
                UserID = userId,
                Title = title,
                Message = message
            };
            _ctx.Notifications.Add(n);
            await _ctx.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
            => await _ctx.Notifications
                         .Where(n => n.UserID == userId)
                         .OrderByDescending(n => n.CreatedAt)
                         .ToListAsync();

        public async Task MarkAsReadAsync(int notificationId)
        {
            var n = await _ctx.Notifications.FindAsync(notificationId);
            if (n != null)
            {
                n.IsRead = true;
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
