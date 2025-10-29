using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ISessionRepository
    {
        IQueryable<Session> GetSessions();
        Session GetById(string sessionId);
        void Add(Session entity);
        void Update(Session entity);
        void Delete(Session entity);

        Task<List<Session>> GetSessionsAsync(CancellationToken cancellationToken = default);
        Task<Session> GetByIdAsync(string sessionId, CancellationToken cancellationToken = default);
        Task AddAsync(Session entity, CancellationToken cancellationToken = default);
    }

}
