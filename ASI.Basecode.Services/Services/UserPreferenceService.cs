using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IUserPreferenceRepository _prefRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserPreferenceService(IUserPreferenceRepository prefRepository, IUnitOfWork unitOfWork)
        {
            _prefRepository = prefRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<UserPreference> QueryByUserId(int userId) => _prefRepository.GetByUser(userId);

        public async Task<List<UserPreference>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _prefRepository.GetByUserAsync(userId, cancellationToken);
        }

        public async Task<UserPreference> GetByIdAsync(int prefId, CancellationToken cancellationToken = default)
        {
            return await _prefRepository.GetByIdAsync(prefId, cancellationToken);
        }

        public async Task CreateAsync(UserPreference pref, CancellationToken cancellationToken = default)
        {
            await _prefRepository.AddAsync(pref, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserPreference pref, CancellationToken cancellationToken = default)
        {
            _prefRepository.Update(pref);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int prefId, CancellationToken cancellationToken = default)
        {
            var entity = await _prefRepository.GetByIdAsync(prefId, cancellationToken);
            if (entity == null) return;
            _prefRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
