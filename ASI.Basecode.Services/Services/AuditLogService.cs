using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuditLogService(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetAuditLogsAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByUserAsync(userId, cancellationToken);
        }

        public async Task<List<AuditLog>> GetAuditLogsByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByEntityTypeAsync(entityType, cancellationToken);
        }

        public async Task<AuditLog> GetAuditLogByIdAsync(int auditLogId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByIdAsync(auditLogId, cancellationToken);
        }

        public async Task CreateAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            auditLog.Timestamp = System.DateTime.UtcNow;
            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

