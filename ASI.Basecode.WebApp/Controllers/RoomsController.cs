using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RoomsController : ASI.Basecode.WebApp.Mvc.ControllerBase<RoomsController>
    {
        private readonly IRoomService _roomService;

        public RoomsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IRoomService roomService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _roomService = roomService;
        }

        // Test endpoint to verify authentication/token and claims without role authorization
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestToken()
        {
            var user = HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            List<object> claims;
            if (user == null)
            {
                claims = new List<object>();
            }
            else
            {
                claims = user.Claims.Select(c => (object)new { c.Type, c.Value }).ToList();
            }

            return Ok(new { isAuthenticated, claims });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _roomService.GetRoomsAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var item = await _roomService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // Admin / SuperAdmin only endpoint to create a room. Accepts a CreateRoomRequest DTO from body.
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Post([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest();

            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Name and Code are required.");

            // Map request to Room entity
            var room = new Room
            {
                // generate a string id (GUID) because DB currently uses string RoomId. If you want numeric auto-increment id,
                // a DB migration is required to change the schema to use an int identity column.
                RoomId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Code = request.Code,
                Seats = request.Seats,
                Location = request.Location,
                Level = request.Level,
                SizeLabel = request.SizeLabel,
                Status = request.Status,
                // serialize operating hours object to JSON to store in OperatingHours string column
                OperatingHours = request.OperatingHours == null ? null : JsonSerializer.Serialize(request.OperatingHours),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoomAmenities = new List<RoomAmenity>(),
            };

            if (request.Amenities != null)
            {
                foreach (var a in request.Amenities.Distinct())
                {
                    room.RoomAmenities.Add(new RoomAmenity { RoomId = room.RoomId, Amenity = a });
                }
            }

            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                room.ImageUrl = request.ImageUrl;
            }

            try
            {
                await _roomService.CreateAsync(room, cancellationToken);
            }
            catch (Exception ex)
            {
                // Return detailed error for debugging (remove in production)
                var messages = new List<string>();
                var e = ex;
                while (e != null)
                {
                    messages.Add(e.Message);
                    e = e.InnerException;
                }
                return StatusCode(500, new { error = "Failed to create room", details = messages });
            }

            // Build response DTO to avoid serialization cycles caused by navigation properties
            var response = new
            {
                room.RoomId,
                room.Name,
                room.Code,
                room.Seats,
                room.Location,
                room.Level,
                room.SizeLabel,
                room.Status,
                OperatingHours = room.OperatingHours,
                room.ImageUrl,
                room.CreatedAt,
                room.UpdatedAt,
                Amenities = room.RoomAmenities?.Select(a => a.Amenity).ToList() ?? new List<string>()
            };

            return CreatedAtAction(nameof(Get), new { id = room.RoomId }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Put(string id, [FromBody] Room model, CancellationToken cancellationToken)
        {
            if (model == null || model.RoomId != id) return BadRequest();
            await _roomService.UpdateAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            await _roomService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }

    // DTO for creating a room from Admin / SuperAdmin UI
    public class CreateRoomRequest
    {
        public string Name { get; set; }
        public string Code { get; set; } // Room number
        public string Location { get; set; }
        public string Level { get; set; }
        public string SizeLabel { get; set; } // Small, Medium, Large
        public int? Seats { get; set; }
        public string Status { get; set; } // Available, Occupied, Under Maintenance
        public OperatingHoursDto OperatingHours { get; set; }
        public List<string> Amenities { get; set; }
        public string ImageUrl { get; set; }
    }

    public class OperatingHoursDto
    {
        public DayHoursDto Weekdays { get; set; }
        public DayHoursDto Weekends { get; set; }
    }

    public class DayHoursDto
    {
        public string Open { get; set; }
        public string Close { get; set; }
    }
}
