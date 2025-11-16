using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminNotificationsController : ASI.Basecode.WebApp.Mvc.ControllerBase<AdminNotificationsController>
    {
        private readonly INotificationService _notificationService;

        public AdminNotificationsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            INotificationService notificationService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get all notifications (Admin and SuperAdmin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can view notifications.");
            }

            var notifications = await _notificationService.GetNotificationsAsync(cancellationToken);
            return Ok(notifications);
        }

        /// <summary>
        /// Get notification by ID (Admin and SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can view notification details.");
            }

            var notification = await _notificationService.GetByIdAsync(id, cancellationToken);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        /// <summary>
        /// Get notifications by user ID (Admin and SuperAdmin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can view user notifications.");
            }

            var notifications = await _notificationService.GetByUserIdAsync(userId, cancellationToken);
            return Ok(notifications);
        }

        /// <summary>
        /// Create notification (Admin and SuperAdmin only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Notification model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can create notifications.");
            }

            if (model == null) return BadRequest();
            await _notificationService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.NotificationId }, model);
        }

        /// <summary>
        /// Update notification (Admin and SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Notification model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can update notifications.");
            }

            if (model == null || model.NotificationId != id) return BadRequest();
            await _notificationService.UpdateAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Mark notification as read (Admin and SuperAdmin only)
        /// </summary>
        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can mark notifications as read.");
            }

            await _notificationService.MarkAsReadAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete notification (Admin and SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin" && userRole?.ToLower() != "admin")
            {
                return Forbid("Only Admin and SuperAdmin can delete notifications.");
            }

            await _notificationService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}

