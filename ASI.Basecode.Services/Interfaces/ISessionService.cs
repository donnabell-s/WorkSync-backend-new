using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ISessionService
    {
        IQueryable<Session> GetSessions();
        Session GetById(string sessionId);
        void Create(Session session);
        void Update(Session session);
        void Delete(string sessionId);

        Task<List<Session>> GetSessionsAsync(CancellationToken cancellationToken = default);
        Task<Session> GetByIdAsync(string sessionId, CancellationToken cancellationToken = default);
        Task CreateAsync(Session session, CancellationToken cancellationToken = default);
        Task UpdateAsync(Session session, CancellationToken cancellationToken = default);
        Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
