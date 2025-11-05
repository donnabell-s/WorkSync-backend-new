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
    public class SuperadminController : ASI.Basecode.WebApp.Mvc.ControllerBase<SuperadminController>
    {
        private readonly ISuperadminService _superadminService;

        public SuperadminController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ISuperadminService superadminService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _superadminService = superadminService;
        }

        /// <summary>
        /// Get all admins
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAdmins(CancellationToken cancellationToken)
        {
            var admins = await _superadminService.GetAllAdminsAsync(cancellationToken);
            return Ok(admins);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = await _superadminService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Get admin by ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAdmin(string userId, CancellationToken cancellationToken)
        {
            var admin = await _superadminService.GetAdminByIdAsync(userId, cancellationToken);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUser(string userId, CancellationToken cancellationToken)
        {
            var user = await _superadminService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Create admin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] User admin, CancellationToken cancellationToken)
        {
            if (admin == null) return BadRequest();
            await _superadminService.CreateAdminAsync(admin, cancellationToken);
            return CreatedAtAction(nameof(GetAdmin), new { userId = admin.UserId }, admin);
        }

        /// <summary>
        /// Update admin
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateAdmin(string userId, [FromBody] User admin, CancellationToken cancellationToken)
        {
            if (admin == null || admin.UserId != userId) return BadRequest();
            await _superadminService.UpdateAdminAsync(admin, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete admin (hard delete)
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAdmin(string userId, CancellationToken cancellationToken)
        {
            await _superadminService.DeleteAdminAsync(userId, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] User user, CancellationToken cancellationToken)
        {
            if (user == null || user.UserId != userId) return BadRequest();
            await _superadminService.UpdateUserAsync(user, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete user (hard delete)
        /// </summary>
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, CancellationToken cancellationToken)
        {
            await _superadminService.DeleteUserAsync(userId, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Get audit logs
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(CancellationToken cancellationToken)
        {
            var auditLogs = await _superadminService.GetAuditLogsAsync(cancellationToken);
            return Ok(auditLogs);
        }

        /// <summary>
        /// Get audit logs by user
        /// </summary>
        [HttpGet("audit-logs/user/{userId}")]
        public async Task<IActionResult> GetAuditLogsByUser(string userId, CancellationToken cancellationToken)
        {
            var auditLogs = await _superadminService.GetAuditLogsByUserAsync(userId, cancellationToken);
            return Ok(auditLogs);
        }
    }
}

