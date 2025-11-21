using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        IEnumerable<Notification> GetAdminNotifications();
        Notification Get(int id);
        void MarkAsRead(int id);
    }
}
//added