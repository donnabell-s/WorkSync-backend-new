using ASI.Basecode.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserPreferenceRepository
    {
        IQueryable<UserPreference> GetPreferences();
        IQueryable<UserPreference> GetByUser(string userId);
        UserPreference GetById(int prefId);
        void Add(UserPreference entity);
        void Update(UserPreference entity);
        void Delete(UserPreference entity);

        Task<List<UserPreference>> GetPreferencesAsync(CancellationToken cancellationToken = default);
        Task<List<UserPreference>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<UserPreference> GetByIdAsync(int prefId, CancellationToken cancellationToken = default);
        Task AddAsync(UserPreference entity, CancellationToken cancellationToken = default);
    }

}
