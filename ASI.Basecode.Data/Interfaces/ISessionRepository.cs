using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ISessionRepository
    {
        IQueryable<Session> GetSessions();
        Session GetById(string sessionId);
        void Add(Session entity);
        void Update(Session entity);
        void Delete(Session entity);
    }

}
