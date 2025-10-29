using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<List<Session>> GetSessionsAsync(CancellationToken cancellationToken = default)
        {
            return await _sessionRepository.GetSessionsAsync(cancellationToken);
        }

        public async Task<Session> GetByIdAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        }

        public async Task CreateAsync(Session session, CancellationToken cancellationToken = default)
        {
            await _sessionRepository.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
        {
            _sessionRepository.Update(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            var entity = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
            if (entity == null) return;
            _sessionRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
