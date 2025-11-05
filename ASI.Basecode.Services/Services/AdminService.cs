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
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminService(IUserRepository userRepository, INotificationRepository notificationRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<User>> GetAdminsAsync(CancellationToken cancellationToken = default)
        {
            var admins = _userRepository.GetUsers().Where(u => u.Role == "Admin" || u.Role == "Superadmin").ToList();
            return await Task.FromResult(admins);
        }

        public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = _userRepository.GetUsers().Where(u => u.Role == "User" || string.IsNullOrEmpty(u.Role)).ToList();
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

        public async Task<List<Notification>> GetAdminNotificationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _notificationRepository.GetByUserAsync(userId, cancellationToken);
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
            if (admin != null && (admin.Role == "Admin" || admin.Role == "Superadmin"))
            {
                admin.IsActive = false;
                admin.UpdatedAt = System.DateTime.UtcNow;
                _userRepository.Update(admin);
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
                user.IsActive = false;
                user.UpdatedAt = System.DateTime.UtcNow;
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

