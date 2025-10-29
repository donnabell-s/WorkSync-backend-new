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
    public class SessionRepository : BaseRepository, ISessionRepository
    {
        public SessionRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<Session> GetSessions() => GetDbSet<Session>();

        public Session GetById(string sessionId) => Context.Set<Session>().Find(sessionId);

        public void Add(Session entity) => GetDbSet<Session>().Add(entity);

        public void Update(Session entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(Session entity) => GetDbSet<Session>().Remove(entity);

        public async Task<List<Session>> GetSessionsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Session>().ToListAsync(cancellationToken);
        }

        public async Task<Session> GetByIdAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<Session>().FindAsync(new object[] { sessionId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(Session entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<Session>().AddAsync(entity, cancellationToken);
        }
    }
}
