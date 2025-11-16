using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<AuditLog> GetAuditLogs() => _auditLogRepository.GetAuditLogs();

        public AuditLog GetById(int auditLogId) => _auditLogRepository.GetById(auditLogId);

        public void Create(AuditLog auditLog)
        {
            auditLog.Timestamp = System.DateTime.UtcNow;
            _auditLogRepository.Add(auditLog);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int auditLogId)
        {
            var entity = _auditLogRepository.GetById(auditLogId);
            if (entity == null) return;
            _auditLogRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetAuditLogsAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<List<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByEntityTypeAsync(entityType, cancellationToken);
        }

        public async Task<AuditLog> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByIdAsync(auditLogId, cancellationToken);
        }

        public async Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            auditLog.Timestamp = System.DateTime.UtcNow;
            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int auditLogId, CancellationToken cancellationToken = default)
        {
            var entity = await _auditLogRepository.GetByIdAsync(auditLogId, cancellationToken);
            if (entity == null) return;
            _auditLogRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

