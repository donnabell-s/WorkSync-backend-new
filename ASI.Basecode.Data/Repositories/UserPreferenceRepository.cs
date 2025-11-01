using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Repositories
{
    public class UserPreferenceRepository : BaseRepository, IUserPreferenceRepository
    {
        public UserPreferenceRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<UserPreference> GetPreferences() => GetDbSet<UserPreference>();

        public IQueryable<UserPreference> GetByUser(int userId) => GetDbSet<UserPreference>().Where(p => p.UserRefId == userId);

        public UserPreference GetById(int prefId) => Context.Set<UserPreference>().Find(prefId);

        public void Add(UserPreference entity) => GetDbSet<UserPreference>().Add(entity);

        public void Update(UserPreference entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(UserPreference entity) => GetDbSet<UserPreference>().Remove(entity);

        public async Task<List<UserPreference>> GetPreferencesAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<UserPreference>().ToListAsync(cancellationToken);
        }

        public async Task<List<UserPreference>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<UserPreference>().Where(p => p.UserRefId == userId).ToListAsync(cancellationToken);
        }

        public async Task<UserPreference> GetByIdAsync(int prefId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<UserPreference>().FindAsync(new object[] { prefId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(UserPreference entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<UserPreference>().AddAsync(entity, cancellationToken);
        }
    }
}
