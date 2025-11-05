using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountService(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<User> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_userRepository.GetById(userId));
        }

        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_userRepository.GetByEmail(email));
        }

        public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            user.UpdatedAt = System.DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                throw new System.Exception("User not found");
            }

            var oldPasswordHash = PasswordManager.EncryptPassword(oldPassword);
            if (user.PasswordHash != oldPasswordHash)
            {
                throw new System.Exception("Invalid old password");
            }

            user.PasswordHash = PasswordManager.EncryptPassword(newPassword);
            user.UpdatedAt = System.DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

