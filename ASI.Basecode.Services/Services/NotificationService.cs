using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public void CreateNotification(string message, string type = "info", int? userId = null)
        {
            var noti = new Notification
            {
                Message = message,
                Type = type,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _repo.Add(noti);
        }

        public IEnumerable<Notification> GetAdminNotifications()
        {
            return _repo.GetAdminNotifications();
        }

        public void MarkAsRead(int id)
        {
            _repo.MarkAsRead(id);
        }
    }
}
//added