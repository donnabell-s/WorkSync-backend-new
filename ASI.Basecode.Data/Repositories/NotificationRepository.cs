using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        public NotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<Notification> GetNotifications() => GetDbSet<Notification>();

        public IQueryable<Notification> GetByUser(string userId) => GetDbSet<Notification>().Where(n => n.UserId == userId);

        public Notification GetById(int notificationId) => Context.Set<Notification>().Find(notificationId);

        public void Add(Notification notification) => GetDbSet<Notification>().Add(notification);

        public void Update(Notification notification) => SetEntityState(notification, EntityState.Modified);

        public void Delete(Notification notification) => GetDbSet<Notification>().Remove(notification);

        public async Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Notification>().ToListAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Notification>().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
        }

        public async Task<Notification> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<Notification>().FindAsync(new object[] { notificationId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            await GetDbSet<Notification>().AddAsync(notification, cancellationToken);
        }
    }
}

