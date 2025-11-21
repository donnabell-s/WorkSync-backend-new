using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Unified Settings Service Interface - combines Account and Preferences management
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Get all settings for a user (account + preferences)
        /// </summary>
        Task<SettingsResult> GetSettingsAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get account settings for a user
        /// </summary>
        Task<User> GetAccountSettingsAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get preferences for a user
        /// </summary>
        Task<List<UserPreference>> GetPreferencesAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update account settings
        /// </summary>
        Task UpdateAccountSettingsAsync(int userId, UpdateAccountSettingsModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update or create preferences for a user
        /// </summary>
        Task<UserPreference> UpdatePreferencesAsync(int userId, UserPreference preference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a preference by ID
        /// </summary>
        Task DeletePreferenceAsync(int preferenceId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result model for GetSettingsAsync
    /// </summary>
    public class SettingsResult
    {
        public User Account { get; set; }
        public List<UserPreference> Preferences { get; set; }
    }

    /// <summary>
    /// Model for updating account settings
    /// </summary>
    public class UpdateAccountSettingsModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

