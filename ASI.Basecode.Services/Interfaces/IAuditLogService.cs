using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAuditLogService
    {
        IQueryable<AuditLog> GetAuditLogs();
        AuditLog GetById(int auditLogId);
        void Create(AuditLog auditLog);
        void Delete(int auditLogId);

        Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<AuditLog> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default);
        Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
        Task DeleteAsync(int auditLogId, CancellationToken cancellationToken = default);
    }
}

