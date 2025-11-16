using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface INotificationService
    {
        IQueryable<Notification> GetNotifications();
        Notification GetById(int notificationId);
        void Create(Notification notification);
        void Update(Notification notification);
        void Delete(int notificationId);

        Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default);
        Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Notification> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default);
        Task CreateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task DeleteAsync(int notificationId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    }
}

