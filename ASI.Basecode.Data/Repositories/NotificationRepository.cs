using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Repositories
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        public NotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<Notification> GetNotifications() => GetDbSet<Notification>();

        public Notification GetById(int notificationId) => Context.Set<Notification>().Find(notificationId);

        public void Add(Notification entity) => GetDbSet<Notification>().Add(entity);

        public void Update(Notification entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(Notification entity) => GetDbSet<Notification>().Remove(entity);

        public async Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Notification>().ToListAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Notification>().Where(n => n.UserRefId == userId).ToListAsync(cancellationToken);
        }

        public async Task<Notification> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<Notification>().FindAsync(new object[] { notificationId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(Notification entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<Notification>().AddAsync(entity, cancellationToken);
        }
    }
}

