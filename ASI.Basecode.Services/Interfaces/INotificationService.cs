using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default);
        Task<List<Notification>> GetNotificationsByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<Notification> GetNotificationByIdAsync(int notificationId, CancellationToken cancellationToken = default);
        Task CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
        Task UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
        Task DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken = default);
    }
}

