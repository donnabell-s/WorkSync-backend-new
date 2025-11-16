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
    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<AuditLog> GetAuditLogs() => GetDbSet<AuditLog>();

        public AuditLog GetById(int auditLogId) => Context.Set<AuditLog>().Find(auditLogId);

        public void Add(AuditLog entity) => GetDbSet<AuditLog>().Add(entity);

        public void Update(AuditLog entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(AuditLog entity) => GetDbSet<AuditLog>().Remove(entity);

        public async Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<AuditLog>().ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<AuditLog>().Where(a => a.UserRefId == userId).ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<AuditLog>().Where(a => a.EntityType == entityType).ToListAsync(cancellationToken);
        }

        public async Task<AuditLog> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<AuditLog>().FindAsync(new object[] { auditLogId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<AuditLog>().AddAsync(entity, cancellationToken);
        }
    }
}

