    using ASI.Basecode.Data.Models;
    using ASI.Basecode.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using AutoMapper;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using System;

    namespace ASI.Basecode.WebApp.Controllers
    {
        [ApiController]
        [Route("api/[controller]/[action]")]

        public class RoomLogsController : ASI.Basecode.WebApp.Mvc.ControllerBase<RoomLogsController>
        {
            private readonly IRoomLogService _logService;

            public RoomLogsController(
                IHttpContextAccessor httpContextAccessor,
                ILoggerFactory loggerFactory,
                IConfiguration configuration,
                IMapper mapper,
                IRoomLogService logService)
                : base(httpContextAccessor, loggerFactory, configuration, mapper)
            {
                _logService = logService;
            }

            /// <summary>
            /// Get all room logs
            /// </summary>
            [HttpGet]
            [AllowAnonymous] // Changed from [Authorize(Policy = "RequireAdmin")]
            public async Task<IActionResult> Get(CancellationToken cancellationToken)
            {
                var items = await _logService.GetRoomLogsAsync(cancellationToken);
            
                var results = items.Select(log => new
                {
                    log.RoomLogId,
                    log.RoomId,
                    RoomName = log.Room?.Name ?? "N/A",
                    RoomCode = log.Room?.Code ?? "N/A",
                    Location = log.Room?.Location ?? "N/A",
                    Capacity = log.Room?.Seats ?? 0,
                    log.UserRefId,
                    UserEmail = log.User?.Email ?? "Unknown",
                    UserName = $"{log.User?.FirstName ?? ""} {log.User?.LastName ?? ""}".Trim(),
                    Action = log.EventType,
                    Status = log.CurrentStatus,
                    Date = log.Timestamp,
                    log.Timestamp
                }).OrderByDescending(x => x.Timestamp).ToList();

                return Ok(results);
            }

            /// <summary>
            /// Get room logs by room ID
            /// </summary>
            [HttpGet("room/{roomId}")]
            public async Task<IActionResult> GetByRoom(string roomId, CancellationToken cancellationToken)
            {
                var items = await _logService.GetByRoomIdAsync(roomId, cancellationToken);
            
                var results = items.Select(log => new
                {
                    log.RoomLogId,
                    log.RoomId,
                    RoomName = log.Room?.Name ?? "N/A",
                    RoomCode = log.Room?.Code ?? "N/A",
                    Location = log.Room?.Location ?? "N/A",
                    Capacity = log.Room?.Seats ?? 0,
                    log.UserRefId,
                    UserEmail = log.User?.Email ?? "Unknown",
                    UserName = $"{log.User?.FirstName ?? ""} {log.User?.LastName ?? ""}".Trim(),
                    Action = log.EventType,
                    Status = log.CurrentStatus,
                    Date = log.Timestamp,
                    log.Timestamp
                }).OrderByDescending(x => x.Timestamp).ToList();

                return Ok(results);
            }

            /// <summary>
            /// Get a specific room log by ID
            /// </summary>
            [HttpGet("{id}")]
            public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
            {
                var item = await _logService.GetByIdAsync(id, cancellationToken);
                if (item == null) return NotFound();

                var result = new
                {
                    item.RoomLogId,
                    item.RoomId,
                    RoomName = item.Room?.Name ?? "N/A",
                    RoomCode = item.Room?.Code ?? "N/A",
                    Location = item.Room?.Location ?? "N/A",
                    Capacity = item.Room?.Seats ?? 0,
                    item.UserRefId,
                    UserEmail = item.User?.Email ?? "Unknown",
                    UserName = $"{item.User?.FirstName ?? ""} {item.User?.LastName ?? ""}".Trim(),
                    Action = item.EventType,
                    Status = item.CurrentStatus,
                    Date = item.Timestamp,
                    item.Timestamp
                };

                return Ok(result);
            }

            /// <summary>
            /// Create a new room log entry (Admin only)
            /// </summary>
            [HttpPost]
            [Authorize(Policy = "RequireAdmin")]
            public async Task<IActionResult> Post([FromBody] CreateRoomLogRequest model, CancellationToken cancellationToken)
            {
                if (model == null) return BadRequest();

                var log = new RoomLog
                {
                    RoomId = model.RoomId,
                    EventType = model.EventType,
                    CurrentStatus = model.CurrentStatus,
                    Timestamp = DateTime.UtcNow
                };

                // Get current user ID from claims if not provided
                var userRefId = GetCurrentUserRefId();
                if (model.UserRefId.HasValue)
                    log.UserRefId = model.UserRefId.Value;
                else if (userRefId.HasValue)
                    log.UserRefId = userRefId;

                await _logService.CreateAsync(log, cancellationToken);
                return CreatedAtAction(nameof(Get), new { id = log.RoomLogId }, log);
            }

            /// <summary>
            /// Delete a room log (Admin only)
            /// </summary>
            [HttpDelete("{id}")]
            [Authorize(Policy = "RequireAdmin")]
            public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
            {
                await _logService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }

            // Helper method to get current user ID from token
            private int? GetCurrentUserRefId()
            {
                var user = HttpContext.User;
                if (user == null || !user.Identity.IsAuthenticated) return null;
            
                var idClaim = user.Claims.FirstOrDefault(c => 
                    c.Type.Equals("UserRefId", StringComparison.OrdinalIgnoreCase) ||
                    c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase) ||
                    c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase) ||
                    c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase) ||
                    c.Type.Equals("id", StringComparison.OrdinalIgnoreCase));
                
                if (idClaim != null && int.TryParse(idClaim.Value, out var idFromClaim)) 
                    return idFromClaim;

                return null;
            }
        }

        // DTO for creating room logs
        public class CreateRoomLogRequest
        {
            public string RoomId { get; set; }
            public int? UserRefId { get; set; }
            public string EventType { get; set; }  // e.g., "Created", "Updated", "StatusChanged", "MaintenanceStarted", "MaintenanceCompleted"
            public string CurrentStatus { get; set; }  // e.g., "Available", "Under Maintenance", "Occupied"
        }
    }   