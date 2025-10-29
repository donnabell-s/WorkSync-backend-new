using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;

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

        public IQueryable<UserPreference> GetByUser(string userId) => _prefRepository.GetByUser(userId);

        public UserPreference GetById(int prefId) => _prefRepository.GetById(prefId);

        public void Create(UserPreference pref)
        {
            _prefRepository.Add(pref);
            _unitOfWork.SaveChanges();
        }

        public void Update(UserPreference pref)
        {
            _prefRepository.Update(pref);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int prefId)
        {
            var entity = _prefRepository.GetById(prefId);
            if (entity == null) return;
            _prefRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }
    }
}
