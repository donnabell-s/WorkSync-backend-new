using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Notification> GetNotifications() => _notificationRepository.GetNotifications();

        public Notification GetById(int notificationId) => _notificationRepository.GetById(notificationId);

        public void Create(Notification notification)
        {
            notification.CreatedAt = System.DateTime.UtcNow;
            notification.IsRead = false;
            _notificationRepository.Add(notification);
            _unitOfWork.SaveChanges();
        }

        public void Update(Notification notification)
        {
            _notificationRepository.Update(notification);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int notificationId)
        {
            var entity = _notificationRepository.GetById(notificationId);
            if (entity == null) return;
            _notificationRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public async Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetNotificationsAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<Notification> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        }

        public async Task CreateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            notification.CreatedAt = System.DateTime.UtcNow;
            notification.IsRead = false;
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var entity = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
            if (entity == null) return;
            _notificationRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var entity = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
            if (entity == null) return;
            entity.IsRead = true;
            entity.ReadAt = System.DateTime.UtcNow;
            _notificationRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

