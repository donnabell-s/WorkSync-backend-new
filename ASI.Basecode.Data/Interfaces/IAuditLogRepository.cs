using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IAuditLogRepository
    {
        IQueryable<AuditLog> GetAuditLogs();
        IQueryable<AuditLog> GetByUser(string userId);
        IQueryable<AuditLog> GetByEntityType(string entityType);
        AuditLog GetById(int auditLogId);
        void Add(AuditLog auditLog);
        void Update(AuditLog auditLog);
        void Delete(AuditLog auditLog);

        Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<AuditLog> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default);
        Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    }
}

