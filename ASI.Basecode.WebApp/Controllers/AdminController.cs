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
    public class AdminController : ASI.Basecode.WebApp.Mvc.ControllerBase<AdminController>
    {
        private readonly IAdminService _adminService;

        public AdminController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IAdminService adminService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get all admins
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAdmins(CancellationToken cancellationToken)
        {
            var admins = await _adminService.GetAdminsAsync(cancellationToken);
            return Ok(admins);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = await _adminService.GetUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Get admin by ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAdmin(string userId, CancellationToken cancellationToken)
        {
            var admin = await _adminService.GetAdminByIdAsync(userId, cancellationToken);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUser(string userId, CancellationToken cancellationToken)
        {
            var user = await _adminService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Get admin notifications
        /// </summary>
        [HttpGet("notifications/{userId}")]
        public async Task<IActionResult> GetAdminNotifications(string userId, CancellationToken cancellationToken)
        {
            var notifications = await _adminService.GetAdminNotificationsAsync(userId, cancellationToken);
            return Ok(notifications);
        }

        /// <summary>
        /// Create admin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] User admin, CancellationToken cancellationToken)
        {
            if (admin == null) return BadRequest();
            await _adminService.CreateAdminAsync(admin, cancellationToken);
            return CreatedAtAction(nameof(GetAdmin), new { userId = admin.UserId }, admin);
        }

        /// <summary>
        /// Update admin
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateAdmin(string userId, [FromBody] User admin, CancellationToken cancellationToken)
        {
            if (admin == null || admin.UserId != userId) return BadRequest();
            await _adminService.UpdateAdminAsync(admin, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete admin (soft delete)
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAdmin(string userId, CancellationToken cancellationToken)
        {
            await _adminService.DeleteAdminAsync(userId, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] User user, CancellationToken cancellationToken)
        {
            if (user == null || user.UserId != userId) return BadRequest();
            await _adminService.UpdateUserAsync(user, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, CancellationToken cancellationToken)
        {
            await _adminService.DeleteUserAsync(userId, cancellationToken);
            return NoContent();
        }
    }
}

