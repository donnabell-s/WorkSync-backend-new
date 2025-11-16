using ASI.Basecode.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IAuditLogRepository
    {
        IQueryable<AuditLog> GetAuditLogs();
        AuditLog GetById(int auditLogId);
        void Add(AuditLog entity);
        void Update(AuditLog entity);
        void Delete(AuditLog entity);

        Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<AuditLog> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default);
        Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default);
    }
}

