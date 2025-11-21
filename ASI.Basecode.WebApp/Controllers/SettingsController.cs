using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Unified Settings Controller - combines Account and Preferences management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase<SettingsController>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserPreferenceService _prefService;
        private readonly IUnitOfWork _unitOfWork;

        public SettingsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserRepository userRepository,
            IUserPreferenceService prefService,
            IUnitOfWork unitOfWork)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userRepository = userRepository;
            _prefService = prefService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all settings for current user (account + preferences)
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMySettings(CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var preferences = await _prefService.GetByUserIdAsync(userId, cancellationToken);

            var result = new
            {
                account = new
                {
                    id = user.Id,
                    userId = user.UserId,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role,
                    isActive = user.IsActive,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt
                },
                preferences = preferences.Select(p => new
                {
                    prefId = p.PrefId,
                    userRefId = p.UserRefId,
                    bookingEmailConfirm = p.BookingEmailConfirm,
                    cancellationNotif = p.CancellationNotif,
                    bookingReminder = p.BookingReminder,
                    reminderTimeMinutes = p.ReminderTimeMinutes,
                    bookingDefaultMinutes = p.BookingDefaultMinutes,
                    rawJson = p.RawJson
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Update account settings
        /// </summary>
        [HttpPut("account")]
        public IActionResult UpdateAccount([FromBody] UpdateAccountViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Update allowed fields
            if (!string.IsNullOrWhiteSpace(model.Fname))
                user.FirstName = model.Fname;

            if (!string.IsNullOrWhiteSpace(model.Lname))
                user.LastName = model.Lname;

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
            {
                // Check if email is already taken
                var existingUser = _userRepository.GetByEmail(model.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return Conflict(new { message = "Email already in use." });
                }
                user.Email = model.Email;
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = Services.Manager.PasswordManager.EncryptPassword(model.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            _unitOfWork.SaveChanges();

            return Ok(new { message = "Account updated successfully." });
        }

        /// <summary>
        /// Get preferences for current user
        /// </summary>
        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var items = await _prefService.GetByUserIdAsync(userId, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Create or update preferences for current user
        /// </summary>
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferenceViewModel model, CancellationToken cancellationToken)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            // If PrefId is provided, update existing preference
            if (model.PrefId.HasValue && model.PrefId.Value > 0)
            {
                var existing = await _prefService.GetByIdAsync(model.PrefId.Value, cancellationToken);
                if (existing == null)
                {
                    return NotFound(new { message = "Preference not found." });
                }

                // Verify ownership
                if (existing.UserRefId != userId)
                {
                    var userRole = User.FindFirstValue(ClaimTypes.Role);
                    if (userRole?.ToLower() != "admin" && userRole?.ToLower() != "superadmin")
                    {
                        return Forbid("You can only update your own preferences unless you are an admin.");
                    }
                }

                existing.BookingEmailConfirm = model.BookingEmailConfirm;
                existing.CancellationNotif = model.CancellationNotif;
                existing.BookingReminder = model.BookingReminder;
                existing.ReminderTimeMinutes = model.ReminderTimeMinutes;
                existing.BookingDefaultMinutes = model.BookingDefaultMinutes;
                existing.RawJson = model.RawJson;

                await _prefService.UpdateAsync(existing, cancellationToken);
                return Ok(new { message = "Preferences updated successfully.", preference = existing });
            }
            else
            {
                // Create new preference
                var preference = new UserPreference
                {
                    UserRefId = userId,
                    BookingEmailConfirm = model.BookingEmailConfirm,
                    CancellationNotif = model.CancellationNotif,
                    BookingReminder = model.BookingReminder,
                    ReminderTimeMinutes = model.ReminderTimeMinutes,
                    BookingDefaultMinutes = model.BookingDefaultMinutes,
                    RawJson = model.RawJson
                };

                await _prefService.CreateAsync(preference, cancellationToken);
                return CreatedAtAction(nameof(GetPreferences), new { id = preference.PrefId }, preference);
            }
        }

        /// <summary>
        /// Delete preference by ID
        /// </summary>
        [HttpDelete("preferences/{id}")]
        public async Task<IActionResult> DeletePreference(int id, CancellationToken cancellationToken)
        {
            var existing = await _prefService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFound(new { message = "Preference not found." });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Check if user owns this preference or is admin
            if (existing.UserRefId.ToString() != currentUserId &&
                userRole?.ToLower() != "admin" && userRole?.ToLower() != "superadmin")
            {
                return Forbid("You can only delete your own preferences unless you are an admin.");
            }

            await _prefService.DeleteAsync(id, cancellationToken);
            return Ok(new { message = "Preference deleted successfully." });
        }
    }
}

