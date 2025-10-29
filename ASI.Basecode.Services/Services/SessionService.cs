using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SessionService(ISessionRepository sessionRepository, IUnitOfWork unitOfWork)
        {
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Session> GetSessions() => _sessionRepository.GetSessions();

        public Session GetById(string sessionId) => _sessionRepository.GetById(sessionId);

        public void Create(Session session)
        {
            _sessionRepository.Add(session);
            _unitOfWork.SaveChanges();
        }

        public void Update(Session session)
        {
            _sessionRepository.Update(session);
            _unitOfWork.SaveChanges();
        }

        public void Delete(string sessionId)
        {
            var entity = _sessionRepository.GetById(sessionId);
            if (entity == null) return;
            _sessionRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }
    }
}
