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
        /// Get all audit logs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _auditLogService.GetAuditLogsAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get audit logs by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken)
        {
            var items = await _auditLogService.GetAuditLogsByUserAsync(userId, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get audit logs by entity type
        /// </summary>
        [HttpGet("entity-type/{entityType}")]
        public async Task<IActionResult> GetByEntityType(string entityType, CancellationToken cancellationToken)
        {
            var items = await _auditLogService.GetAuditLogsByEntityTypeAsync(entityType, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Create audit log
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AuditLog model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _auditLogService.CreateAuditLogAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.AuditLogId }, model);
        }
    }
}

