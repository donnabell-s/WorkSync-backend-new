using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserPreferenceService
    {
        // Queryable access for advanced queries
        IQueryable<UserPreference> QueryByUserId(int userId);

        // Async convenience methods for common operations
        Task<List<UserPreference>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserPreference> GetByIdAsync(int prefId, CancellationToken cancellationToken = default);

        Task CreateAsync(UserPreference pref, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserPreference pref, CancellationToken cancellationToken = default);
        Task DeleteAsync(int prefId, CancellationToken cancellationToken = default);
    }
}
