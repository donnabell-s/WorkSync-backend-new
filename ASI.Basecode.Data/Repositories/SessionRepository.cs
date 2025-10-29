using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class SessionRepository : BaseRepository, ISessionRepository
    {
        public SessionRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<Session> GetSessions() => GetDbSet<Session>();

        public Session GetById(string sessionId) => Context.Set<Session>().Find(sessionId);

        public void Add(Session entity) => GetDbSet<Session>().Add(entity);

        public void Update(Session entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(Session entity) => GetDbSet<Session>().Remove(entity);
    }
}
