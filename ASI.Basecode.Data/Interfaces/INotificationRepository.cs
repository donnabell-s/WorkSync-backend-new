using ASI.Basecode.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface INotificationRepository
    {
        IQueryable<Notification> GetNotifications();
        Notification GetById(int notificationId);
        void Add(Notification entity);
        void Update(Notification entity);
        void Delete(Notification entity);

        Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default);
        Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Notification> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default);
        Task AddAsync(Notification entity, CancellationToken cancellationToken = default);
    }
}

