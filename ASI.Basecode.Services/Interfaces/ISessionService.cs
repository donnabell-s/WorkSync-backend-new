using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ISessionService
    {
        IQueryable<Session> GetSessions();
        Session GetById(string sessionId);
        void Create(Session session);
        void Update(Session session);
        void Delete(string sessionId);
    }
}
