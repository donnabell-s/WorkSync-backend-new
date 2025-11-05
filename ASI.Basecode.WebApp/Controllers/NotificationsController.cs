using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NotificationsController : ASI.Basecode.WebApp.Mvc.ControllerBase<NotificationsController>
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
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
        /// Get all notifications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _notificationService.GetNotificationsAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get notifications by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken)
        {
            var items = await _notificationService.GetNotificationsByUserAsync(userId, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _notificationService.GetNotificationByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Create notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Notification model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _notificationService.CreateNotificationAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.NotificationId }, model);
        }

        /// <summary>
        /// Update notification
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Notification model, CancellationToken cancellationToken)
        {
            if (model == null || model.NotificationId != id) return BadRequest();
            await _notificationService.UpdateNotificationAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
        {
            await _notificationService.MarkAsReadAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        [HttpPut("mark-all-read/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(string userId, CancellationToken cancellationToken)
        {
            await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _notificationService.DeleteNotificationAsync(id, cancellationToken);
            return NoContent();
        }
    }
}

