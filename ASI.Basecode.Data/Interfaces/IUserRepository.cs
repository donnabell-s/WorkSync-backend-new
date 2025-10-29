using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        User GetById(string userId);
        User GetByEmail(string email);
        void Add(User user);
        void Update(User user);
        void Delete(User user);
    }
}
