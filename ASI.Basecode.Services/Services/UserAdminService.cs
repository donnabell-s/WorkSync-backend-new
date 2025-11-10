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
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserAdminService(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // User CRUD
        public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = _userRepository.GetUsers()
                .Where(u => u.Role == "User" || string.IsNullOrEmpty(u.Role) || u.Role.ToLower() == "user")
                .ToList();
            return await Task.FromResult(users);
        }

        public async Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_userRepository.GetById(id));
        }

        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_userRepository.GetByEmail(email));
        }

        public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            user.Role = "User";
            user.PasswordHash = PasswordManager.EncryptPassword(user.PasswordHash);
            user.CreatedAt = System.DateTime.UtcNow;
            user.IsActive = true;
            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            user.UpdatedAt = System.DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = _userRepository.GetById(id);
            if (user != null)
            {
                user.IsActive = false;
                user.UpdatedAt = System.DateTime.UtcNow;
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        // Admin CRUD
        public async Task<List<User>> GetAdminsAsync(CancellationToken cancellationToken = default)
        {
            var admins = _userRepository.GetUsers()
                .Where(u => u.Role == "Admin" || u.Role == "Superadmin")
                .ToList();
            return await Task.FromResult(admins);
        }

        public async Task<User> GetAdminByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var admin = _userRepository.GetById(id);
            if (admin != null && (admin.Role == "Admin" || admin.Role == "Superadmin"))
            {
                return await Task.FromResult(admin);
            }
            return null;
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

        public async Task DeleteAdminAsync(int id, CancellationToken cancellationToken = default)
        {
            var admin = _userRepository.GetById(id);
            if (admin != null && (admin.Role == "Admin" || admin.Role == "Superadmin"))
            {
                admin.IsActive = false;
                admin.UpdatedAt = System.DateTime.UtcNow;
                _userRepository.Update(admin);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
