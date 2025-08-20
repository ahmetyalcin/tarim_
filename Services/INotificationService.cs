using System.Collections.Generic;
using System.Threading.Tasks;
using TarimHibe.Models;

namespace TarimHibe.Services
{
    public interface INotificationService
    {
        Task SendAsync(int userId, string title, string message);
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
    }
}