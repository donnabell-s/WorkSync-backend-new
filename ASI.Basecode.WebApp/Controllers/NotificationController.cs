using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        // GET: /api/notification/admin
        [HttpGet("admin")]
        public IActionResult GetAdminNotifications()
        {
            var notifications = _service.GetAdminNotifications();
            return Ok(notifications);
        }

        // POST: /api/notification
        [HttpPost]
        public IActionResult CreateNotification([FromBody] NotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
                return BadRequest("Message is required.");

            _service.CreateNotification(request.Message, request.Type, request.UserId);
            return Ok(new { success = true });
        }

        // PATCH: /api/notification/{id}/read
        [HttpPatch("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            _service.MarkAsRead(id);
            return Ok(new { success = true });
        }
    }

    public class NotificationRequest
    {
        public string Message { get; set; }
        public string Type { get; set; } = "info";
        public int? UserId { get; set; }
    }
}
//added