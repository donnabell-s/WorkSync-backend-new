using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public User GetById(string userId) => Context.Set<User>().Find(userId);

        public User GetByEmail(string email) => GetDbSet<User>().FirstOrDefault(u => u.Email == email);

        public void Add(User user) => GetDbSet<User>().Add(user);

        public void Update(User user) => SetEntityState(user, EntityState.Modified);

        public void Delete(User user) => GetDbSet<User>().Remove(user);

    }
}
