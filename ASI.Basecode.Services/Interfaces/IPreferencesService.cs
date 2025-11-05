using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IPreferencesService
    {
        Task<List<UserPreference>> GetPreferencesByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<UserPreference> GetPreferenceByIdAsync(int prefId, CancellationToken cancellationToken = default);
        Task CreatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default);
        Task UpdatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default);
        Task DeletePreferenceAsync(int prefId, CancellationToken cancellationToken = default);
    }
}

