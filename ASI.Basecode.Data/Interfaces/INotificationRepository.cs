using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface INotificationRepository
    {
        IQueryable<Notification> GetNotifications();
        IQueryable<Notification> GetByUser(string userId);
        Notification GetById(int notificationId);
        void Add(Notification notification);
        void Update(Notification notification);
        void Delete(Notification notification);

        Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default);
        Task<List<Notification>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<Notification> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default);
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    }
}

