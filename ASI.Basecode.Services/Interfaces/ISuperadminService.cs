using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ISuperadminService
    {
        Task<List<User>> GetAllAdminsAsync(CancellationToken cancellationToken = default);
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<User> GetAdminByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<User> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task CreateAdminAsync(User admin, CancellationToken cancellationToken = default);
        Task UpdateAdminAsync(User admin, CancellationToken cancellationToken = default);
        Task DeleteAdminAsync(string userId, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
        Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}

