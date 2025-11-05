using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetNotificationsAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetNotificationsByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetByUserAsync(userId, cancellationToken);
        }

        public async Task<Notification> GetNotificationByIdAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        }

        public async Task CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            notification.CreatedAt = System.DateTime.UtcNow;
            notification.IsRead = false;
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = System.DateTime.UtcNow;
                _notificationRepository.Update(notification);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _notificationRepository.GetByUserAsync(userId, cancellationToken);
            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = System.DateTime.UtcNow;
                _notificationRepository.Update(notification);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
            if (notification != null)
            {
                _notificationRepository.Delete(notification);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

