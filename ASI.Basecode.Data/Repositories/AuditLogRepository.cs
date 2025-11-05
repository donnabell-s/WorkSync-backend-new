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
    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<AuditLog> GetAuditLogs() => GetDbSet<AuditLog>();

        public IQueryable<AuditLog> GetByUser(string userId) => GetDbSet<AuditLog>().Where(a => a.UserId == userId);

        public IQueryable<AuditLog> GetByEntityType(string entityType) => GetDbSet<AuditLog>().Where(a => a.EntityType == entityType);

        public AuditLog GetById(int auditLogId) => Context.Set<AuditLog>().Find(auditLogId);

        public void Add(AuditLog auditLog) => GetDbSet<AuditLog>().Add(auditLog);

        public void Update(AuditLog auditLog) => SetEntityState(auditLog, EntityState.Modified);

        public void Delete(AuditLog auditLog) => GetDbSet<AuditLog>().Remove(auditLog);

        public async Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<AuditLog>().OrderByDescending(a => a.Timestamp).ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<AuditLog>().Where(a => a.UserId == userId).OrderByDescending(a => a.Timestamp).ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<AuditLog>().Where(a => a.EntityType == entityType).OrderByDescending(a => a.Timestamp).ToListAsync(cancellationToken);
        }

        public async Task<AuditLog> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<AuditLog>().FindAsync(new object[] { auditLogId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            await GetDbSet<AuditLog>().AddAsync(auditLog, cancellationToken);
        }
    }
}

