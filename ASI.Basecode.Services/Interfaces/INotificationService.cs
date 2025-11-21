using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface INotificationService
    {
        void CreateNotification(string message, string type = "info", int? userId = null);
        IEnumerable<Notification> GetAdminNotifications();
        void MarkAsRead(int id);
    }
}
//added