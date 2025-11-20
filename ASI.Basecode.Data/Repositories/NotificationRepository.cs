using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly WorkSyncDbContext _context;

        public NotificationRepository(WorkSyncDbContext context)
        {
            _context = context;
        }

        public void Add(Notification notification)
        {
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public IEnumerable<Notification> GetAdminNotifications()
        {
            return _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public Notification Get(int id)
        {
            return _context.Notifications.FirstOrDefault(n => n.Id == id);
        }

        public void MarkAsRead(int id)
        {
            var noti = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (noti != null)
            {
                noti.IsRead = true;
                _context.SaveChanges();
            }
        }
    }
}
//added