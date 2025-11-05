using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetAuditLogsByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<AuditLog> GetAuditLogByIdAsync(int auditLogId, CancellationToken cancellationToken = default);
        Task CreateAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    }
}

