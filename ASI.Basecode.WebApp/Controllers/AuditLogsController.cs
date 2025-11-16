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
    public class AuditLogsController : ASI.Basecode.WebApp.Mvc.ControllerBase<AuditLogsController>
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IAuditLogService auditLogService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Get all audit logs (SuperAdmin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view audit logs.");
            }

            var auditLogs = await _auditLogService.GetAuditLogsAsync(cancellationToken);
            return Ok(auditLogs);
        }

        /// <summary>
        /// Get audit log by ID (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view audit log details.");
            }

            var auditLog = await _auditLogService.GetByIdAsync(id, cancellationToken);
            if (auditLog == null) return NotFound();
            return Ok(auditLog);
        }

        /// <summary>
        /// Get audit logs by user ID (SuperAdmin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view user audit logs.");
            }

            var auditLogs = await _auditLogService.GetByUserIdAsync(userId, cancellationToken);
            return Ok(auditLogs);
        }

        /// <summary>
        /// Get audit logs by entity type (SuperAdmin only)
        /// </summary>
        [HttpGet("entity/{entityType}")]
        public async Task<IActionResult> GetByEntityType(string entityType, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view entity audit logs.");
            }

            var auditLogs = await _auditLogService.GetByEntityTypeAsync(entityType, cancellationToken);
            return Ok(auditLogs);
        }

        /// <summary>
        /// Create audit log (SuperAdmin only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditLog model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can create audit logs.");
            }

            if (model == null) return BadRequest();
            await _auditLogService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.AuditLogId }, model);
        }

        /// <summary>
        /// Delete audit log (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can delete audit logs.");
            }

            await _auditLogService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}

