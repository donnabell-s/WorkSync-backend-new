using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserAdminService
    {
        // User CRUD
        Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int id, CancellationToken cancellationToken = default);

        // Admin CRUD
        Task<List<User>> GetAdminsAsync(CancellationToken cancellationToken = default);
        Task<User> GetAdminByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateAdminAsync(User admin, CancellationToken cancellationToken = default);
        Task UpdateAdminAsync(User admin, CancellationToken cancellationToken = default);
        Task DeleteAdminAsync(int id, CancellationToken cancellationToken = default);
    }
}
