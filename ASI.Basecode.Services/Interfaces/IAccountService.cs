using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(string userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
    }
}

