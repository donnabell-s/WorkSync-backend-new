using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserPreferenceService
    {
        IQueryable<UserPreference> GetByUser(string userId);
        UserPreference GetById(int prefId);
        void Create(UserPreference pref);
        void Update(UserPreference pref);
        void Delete(int prefId);
    }
}
