using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class SuperadminService : ISuperadminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SuperadminService(IUserRepository userRepository, IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<User>> GetAllAdminsAsync(CancellationToken cancellationToken = default)
        {
            var admins = _userRepository.GetUsers().Where(u => u.Role == "Admin" || u.Role == "Superadmin").ToList();
            return await Task.FromResult(admins);
        }

        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = _userRepository.GetUsers().ToList();
            return await Task.FromResult(users);
        }

        public async Task<User> GetAdminByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var admin = _userRepository.GetById(userId);
            if (admin != null && (admin.Role == "Admin" || admin.Role == "Superadmin"))
            {
                return await Task.FromResult(admin);
            }
            return null;
        }

        public async Task<User> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_userRepository.GetById(userId));
        }

        public async Task CreateAdminAsync(User admin, CancellationToken cancellationToken = default)
        {
            admin.Role = "Admin";
            admin.PasswordHash = PasswordManager.EncryptPassword(admin.PasswordHash);
            admin.CreatedAt = System.DateTime.UtcNow;
            admin.IsActive = true;
            _userRepository.Add(admin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAdminAsync(User admin, CancellationToken cancellationToken = default)
        {
            admin.UpdatedAt = System.DateTime.UtcNow;
            _userRepository.Update(admin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAdminAsync(string userId, CancellationToken cancellationToken = default)
        {
            var admin = _userRepository.GetById(userId);
            if (admin != null)
            {
                _userRepository.Delete(admin);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            user.UpdatedAt = System.DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = _userRepository.GetById(userId);
            if (user != null)
            {
                _userRepository.Delete(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetAuditLogsAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByUserAsync(userId, cancellationToken);
        }
    }
}

