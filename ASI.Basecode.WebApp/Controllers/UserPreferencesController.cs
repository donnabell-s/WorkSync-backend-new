using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class UserPreferencesController : ASI.Basecode.WebApp.Mvc.ControllerBase<UserPreferencesController>
    {
        private readonly IUserPreferenceService _prefService;

        public UserPreferencesController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserPreferenceService prefService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _prefService = prefService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken)
        {
            int parsedUserId;
            try
            {
                parsedUserId = int.Parse(userId);
            }
            catch
            {
                return BadRequest(new { message = "Invalid user id." });
            }

            // Check if user is accessing their own preferences or is an admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (currentUserId != parsedUserId.ToString() && userRole?.ToLower() != "admin" && userRole?.ToLower() != "superadmin")
            {
                return Forbid("You can only access your own preferences unless you are an admin.");
            }

            var items = await _prefService.GetByUserAsync(parsedUserId, cancellationToken);
            return Ok(items);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyPreferences(CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var items = await _prefService.GetByUserAsync(userId, cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _prefService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPreferenceViewModel model, CancellationToken cancellationToken)
        {
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            // Ensure user can only create preferences for themselves unless admin
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (model.UserRefId.HasValue && model.UserRefId.Value != userId && 
                userRole?.ToLower() != "admin" && userRole?.ToLower() != "superadmin")
            {
                return Forbid("You can only create preferences for yourself unless you are an admin.");
            }

            var preference = new UserPreference
            {
                UserRefId = model.UserRefId ?? userId,
                BookingEmailConfirm = model.BookingEmailConfirm,
                CancellationNotif = model.CancellationNotif,
                BookingReminder = model.BookingReminder,
                ReminderTimeMinutes = model.ReminderTimeMinutes,
                BookingDefaultMinutes = model.BookingDefaultMinutes,
                RawJson = model.RawJson
            };

            await _prefService.CreateAsync(preference, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = preference.PrefId }, preference);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserPreferenceViewModel model, CancellationToken cancellationToken)
        {
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _prefService.GetByIdAsync(id, cancellationToken);
            if (existing == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            // Check if user owns this preference or is admin
            if (existing.UserRefId.ToString() != currentUserId && 
                userRole?.ToLower() != "admin" && userRole?.ToLower() != "superadmin")
            {
                return Forbid("You can only update your own preferences unless you are an admin.");
            }

            existing.BookingEmailConfirm = model.BookingEmailConfirm;
            existing.CancellationNotif = model.CancellationNotif;
            existing.BookingReminder = model.BookingReminder;
            existing.ReminderTimeMinutes = model.ReminderTimeMinutes;
            existing.BookingDefaultMinutes = model.BookingDefaultMinutes;
            existing.RawJson = model.RawJson;

            await _prefService.UpdateAsync(existing, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var existing = await _prefService.GetByIdAsync(id, cancellationToken);
            if (existing == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            // Check if user owns this preference or is admin
            if (existing.UserRefId.ToString() != currentUserId && 
                userRole?.ToLower() != "admin" && userRole?.ToLower() != "superadmin")
            {
                return Forbid("You can only delete your own preferences unless you are an admin.");
            }

            await _prefService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
