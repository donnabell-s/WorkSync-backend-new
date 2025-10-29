using ASI.Basecode.Data.Models;
using System.Linq;

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
    }

}
