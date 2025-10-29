using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserPreferenceService
    {
        IQueryable<UserPreference> GetByUser(string userId);
        UserPreference GetById(int prefId);
        void Create(UserPreference pref);
        void Update(UserPreference pref);
        void Delete(int prefId);

        Task<List<UserPreference>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<UserPreference> GetByIdAsync(int prefId, CancellationToken cancellationToken = default);
        Task CreateAsync(UserPreference pref, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserPreference pref, CancellationToken cancellationToken = default);
        Task DeleteAsync(int prefId, CancellationToken cancellationToken = default);
    }
}
