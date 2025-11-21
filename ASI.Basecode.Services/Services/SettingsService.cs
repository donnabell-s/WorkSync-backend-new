using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Unified Settings Service - combines Account and Preferences management
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserPreferenceService _preferenceService;
        private readonly IUnitOfWork _unitOfWork;

        public SettingsService(
            IUserRepository userRepository,
            IUserPreferenceService preferenceService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _preferenceService = preferenceService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all settings for a user (account + preferences)
        /// </summary>
        public async Task<SettingsResult> GetSettingsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var account = _userRepository.GetById(userId);
            if (account == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
            }

            var preferences = await _preferenceService.GetByUserIdAsync(userId, cancellationToken);

            return new SettingsResult
            {
                Account = account,
                Preferences = preferences
            };
        }

        /// <summary>
        /// Get account settings for a user
        /// </summary>
        public async Task<User> GetAccountSettingsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
            }

            return await Task.FromResult(user);
        }

        /// <summary>
        /// Get preferences for a user
        /// </summary>
        public async Task<List<UserPreference>> GetPreferencesAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _preferenceService.GetByUserIdAsync(userId, cancellationToken);
        }

        /// <summary>
        /// Update account settings
        /// </summary>
        public async Task UpdateAccountSettingsAsync(int userId, UpdateAccountSettingsModel model, CancellationToken cancellationToken = default)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
            }

            // Update allowed fields
            if (!string.IsNullOrWhiteSpace(model.FirstName))
            {
                user.FirstName = model.FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.LastName))
            {
                user.LastName = model.LastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
            {
                // Check if email is already taken
                var existingUser = _userRepository.GetByEmail(model.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    throw new InvalidOperationException("Email is already in use by another user.");
                }
                user.Email = model.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = PasswordManager.EncryptPassword(model.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update or create preferences for a user
        /// </summary>
        public async Task<UserPreference> UpdatePreferencesAsync(int userId, UserPreference preference, CancellationToken cancellationToken = default)
        {
            if (preference == null)
            {
                throw new ArgumentNullException(nameof(preference));
            }

            // Ensure the preference belongs to the user
            preference.UserRefId = userId;

            // If PrefId is provided and > 0, update existing preference
            if (preference.PrefId > 0)
            {
                var existing = await _preferenceService.GetByIdAsync(preference.PrefId, cancellationToken);
                if (existing == null)
                {
                    throw new ArgumentException($"Preference with ID {preference.PrefId} not found.", nameof(preference));
                }

                // Verify ownership
                if (existing.UserRefId != userId)
                {
                    throw new UnauthorizedAccessException("You can only update your own preferences.");
                }

                // Update existing preference
                existing.BookingEmailConfirm = preference.BookingEmailConfirm;
                existing.CancellationNotif = preference.CancellationNotif;
                existing.BookingReminder = preference.BookingReminder;
                existing.ReminderTimeMinutes = preference.ReminderTimeMinutes;
                existing.BookingDefaultMinutes = preference.BookingDefaultMinutes;
                existing.RawJson = preference.RawJson;

                await _preferenceService.UpdateAsync(existing, cancellationToken);
                return existing;
            }
            else
            {
                // Create new preference
                await _preferenceService.CreateAsync(preference, cancellationToken);
                return preference;
            }
        }

        /// <summary>
        /// Delete a preference by ID
        /// </summary>
        public async Task DeletePreferenceAsync(int preferenceId, CancellationToken cancellationToken = default)
        {
            var existing = await _preferenceService.GetByIdAsync(preferenceId, cancellationToken);
            if (existing == null)
            {
                throw new ArgumentException($"Preference with ID {preferenceId} not found.", nameof(preferenceId));
            }

            await _preferenceService.DeleteAsync(preferenceId, cancellationToken);
        }
    }
}

