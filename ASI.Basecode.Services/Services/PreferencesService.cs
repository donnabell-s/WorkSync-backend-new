using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class PreferencesService : IPreferencesService
    {
        private readonly IUserPreferenceRepository _preferenceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PreferencesService(IUserPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _preferenceRepository = preferenceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UserPreference>> GetPreferencesByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _preferenceRepository.GetByUserAsync(userId, cancellationToken);
        }

        public async Task<UserPreference> GetPreferenceByIdAsync(int prefId, CancellationToken cancellationToken = default)
        {
            return await _preferenceRepository.GetByIdAsync(prefId, cancellationToken);
        }

        public async Task CreatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default)
        {
            await _preferenceRepository.AddAsync(preference, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default)
        {
            _preferenceRepository.Update(preference);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePreferenceAsync(int prefId, CancellationToken cancellationToken = default)
        {
            var preference = await _preferenceRepository.GetByIdAsync(prefId, cancellationToken);
            if (preference != null)
            {
                _preferenceRepository.Delete(preference);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

