using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<User>> GetAdminsAsync(CancellationToken cancellationToken = default);
        Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<User> GetAdminByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<User> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetAdminNotificationsAsync(string userId, CancellationToken cancellationToken = default);
        Task CreateAdminAsync(User admin, CancellationToken cancellationToken = default);
        Task UpdateAdminAsync(User admin, CancellationToken cancellationToken = default);
        Task DeleteAdminAsync(string userId, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}

