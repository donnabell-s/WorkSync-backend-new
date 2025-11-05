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
using System.Security.Claims;
using System.Globalization;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BookingsController : ASI.Basecode.WebApp.Mvc.ControllerBase<BookingsController>
    {
        private readonly IBookingService _bookingService;

        public BookingsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IBookingService bookingService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _bookingService = bookingService;
        }

        // DTOs
        public class RecurrenceDto
        {
            public bool IsRecurring { get; set; }
            public string Pattern { get; set; } // "daily", "weekly", "monthly"
            public int? Interval { get; set; } // e.g., every 2 weeks
            public List<int> DaysOfWeek { get; set; } // 0=Sunday .. 6=Saturday for weekly
            public DateTime? EndDate { get; set; }
        }

        public class CreateBookingRequest
        {
            public string RoomId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDatetime { get; set; }
            public DateTime EndDatetime { get; set; }
            // Accept arbitrary JSON for recurrence to avoid model binder converting to string
            public JsonElement Recurrence { get; set; }
        }

        public class RoomOperatingHoursDto
        {
            public DayHoursDto Weekdays { get; set; }
            public DayHoursDto Weekends { get; set; }
        }

        public class DayHoursDto
        {
            public string Open { get; set; }
            public string Close { get; set; }
        }

        // Helpers
        private int? GetCurrentUserRefId()
        {
            var user = HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated) return null;
            // Try common claim names
            var idClaim = user.Claims.FirstOrDefault(c => c.Type.Equals("UserRefId", StringComparison.OrdinalIgnoreCase)
                                                        || c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                                                        || c.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
                                                        || c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            if (idClaim == null) return null;
            if (int.TryParse(idClaim.Value, out var id)) return id;
            return null;
        }

        private bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return aStart < bEnd && bStart < aEnd;
        }

        // Check conflicts for a single occurrence against existing bookings for the same room
        private bool HasConflict(IEnumerable<Booking> existing, DateTime start, DateTime end)
        {
            foreach (var b in existing)
            {
                if (b.StartDatetime == null || b.EndDatetime == null) continue;
                if (Overlaps(start, end, b.StartDatetime.Value, b.EndDatetime.Value) && !string.Equals(b.Status, "Declined", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // Generate occurrences for recurrence until endDate (inclusive) or limit (to avoid infinite)
        private IEnumerable<(DateTime start, DateTime end)> GenerateOccurrences(CreateBookingRequest req, int maxOccurrences = 365)
        {
            var list = new List<(DateTime, DateTime)>();
            RecurrenceDto rec = null;
            try
            {
                if (req.Recurrence.ValueKind != JsonValueKind.Undefined && req.Recurrence.ValueKind != JsonValueKind.Null)
                    rec = req.Recurrence.Deserialize<RecurrenceDto>();
            }
            catch { rec = null; }

            if (rec == null || !rec.IsRecurring)
            {
                list.Add((req.StartDatetime, req.EndDatetime));
                return list;
            }

            var pattern = rec.Pattern?.ToLowerInvariant();
            var interval = rec.Interval.GetValueOrDefault(1);
            var endDate = rec.EndDate ?? req.StartDatetime.AddMonths(6); // default 6 months

            var currentStart = req.StartDatetime;
            var currentEnd = req.EndDatetime;
            int occurrences = 0;

            while (currentStart <= endDate && occurrences < maxOccurrences)
            {
                if (pattern == "daily")
                {
                    list.Add((currentStart, currentEnd));
                    currentStart = currentStart.AddDays(interval);
                    currentEnd = currentEnd.AddDays(interval);
                }
                else if (pattern == "weekly")
                {
                    // If specific days are provided, generate for the week range
                    var days = rec.DaysOfWeek ?? new List<int> { (int)currentStart.DayOfWeek };
                    var weekStart = currentStart.Date;
                    // For each week, add occurrences matching days
                    foreach (var d in days)
                    {
                        int targetDow = d % 7; // 0..6
                        int currentDow = (int)weekStart.DayOfWeek;
                        int diff = (targetDow - currentDow + 7) % 7;
                        var occStart = weekStart.AddDays(diff).Add(currentStart.TimeOfDay);
                        var occEnd = occStart + (currentEnd - currentStart);
                        if (occStart <= endDate) list.Add((occStart, occEnd));
                    }
                    weekStart = weekStart.AddDays(7 * interval);
                    currentStart = weekStart + currentStart.TimeOfDay;
                    currentEnd = currentStart + (req.EndDatetime - req.StartDatetime);
                }
                else if (pattern == "monthly")
                {
                    list.Add((currentStart, currentEnd));
                    currentStart = currentStart.AddMonths(interval);
                    currentEnd = currentEnd.AddMonths(interval);
                }
                else
                {
                    // unknown pattern - treat as single
                    list.Add((currentStart, currentEnd));
                    break;
                }
                occurrences++;
            }

            return list;
        }

        // GET endpoints
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _bookingService.GetBookingsAsync(cancellationToken);

            var results = items.Select(b => new
            {
                b.BookingId,
                b.RoomId,
                b.UserRefId,
                b.Title,
                b.Description,
                b.StartDatetime,
                b.EndDatetime,
                b.Recurrence,
                b.Status,
                b.CreatedAt,
                b.UpdatedAt,
                BookingLogs = b.BookingLogs?.Select(bl => new { bl.BookingLogId, bl.EventType, bl.Timestamp }).ToList(),
                Room = b.Room == null ? null : new
                {
                    b.Room.RoomId,
                    b.Room.Name,
                    b.Room.Code,
                    b.Room.Seats,
                    b.Room.Location,
                    b.Room.Level,
                    b.Room.SizeLabel,
                    b.Room.Status,
                    OperatingHours = b.Room.OperatingHours,
                    b.Room.ImageUrl
                }
            }).ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _bookingService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();

            var result = new
            {
                item.BookingId,
                item.RoomId,
                item.UserRefId,
                item.Title,
                item.Description,
                item.StartDatetime,
                item.EndDatetime,
                item.Recurrence,
                item.Status,
                item.CreatedAt,
                item.UpdatedAt,
                BookingLogs = item.BookingLogs?.Select(bl => new { bl.BookingLogId, bl.EventType, bl.Timestamp }).ToList(),
                Room = item.Room == null ? null : new
                {
                    item.Room.RoomId,
                    item.Room.Name,
                    item.Room.Code,
                    item.Room.Seats,
                    item.Room.Location,
                    item.Room.Level,
                    item.Room.SizeLabel,
                    item.Room.Status,
                    OperatingHours = item.Room.OperatingHours,
                    item.Room.ImageUrl
                }
            };

            return Ok(result);
        }

        // User endpoints
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest();
            if (request.StartDatetime >= request.EndDatetime) return BadRequest("Start must be before End");

            // Get existing bookings for the room
            var existing = (await _bookingService.GetBookingsAsync(cancellationToken)).Where(b => b.RoomId == request.RoomId).ToList();

            // Check occurrences
            var occurrences = GenerateOccurrences(request, 365).ToList();
            foreach (var occ in occurrences)
            {
                if (HasConflict(existing, occ.start, occ.end))
                {
                    return Conflict(new { message = "Requested time (or recurring series) conflicts with existing bookings" });
                }
            }

            // Get room via RoomService through controller base services
            // Use service locator pattern via HttpContext.RequestServices
            var roomService = HttpContext.RequestServices.GetService(typeof(ASI.Basecode.Services.Interfaces.IRoomService)) as ASI.Basecode.Services.Interfaces.IRoomService;
            if (roomService != null)
            {
                var room = await roomService.GetByIdAsync(request.RoomId, cancellationToken);
                if (room == null) return NotFound(new { message = "Room not found" });

                if (!string.IsNullOrWhiteSpace(room.OperatingHours))
                {
                    RoomOperatingHoursDto ops = null;
                    try { ops = JsonSerializer.Deserialize<RoomOperatingHoursDto>(room.OperatingHours); } catch { ops = null; }

                    if (ops != null)
                    {
                        foreach (var occ in occurrences)
                        {
                            var day = occ.start.DayOfWeek;
                            DayHoursDto dayHours = (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) ? ops.Weekends : ops.Weekdays;
                            if (dayHours == null || string.IsNullOrWhiteSpace(dayHours.Open) || string.IsNullOrWhiteSpace(dayHours.Close))
                            {
                                return BadRequest(new { message = "Room operating hours are not configured for this day" });
                            }

                            if (!TimeSpan.TryParse(dayHours.Open, CultureInfo.InvariantCulture, out var openTs) || !TimeSpan.TryParse(dayHours.Close, CultureInfo.InvariantCulture, out var closeTs))
                            {
                                return BadRequest(new { message = "Room operating hours time format is invalid" });
                            }

                            var occStartTs = occ.start.TimeOfDay;
                            var occEndTs = occ.end.TimeOfDay;

                            if (occStartTs < openTs || occEndTs > closeTs)
                            {
                                return BadRequest(new { message = $"Requested time {occ.start} - {occ.end} is outside room operating hours ({dayHours.Open} - {dayHours.Close})" });
                            }
                        }
                    }
                }
            }

            var booking = new Booking
            {
                RoomId = request.RoomId,
                Title = request.Title,
                Description = request.Description,
                StartDatetime = request.StartDatetime,
                EndDatetime = request.EndDatetime,
                Recurrence = (request.Recurrence.ValueKind == JsonValueKind.Undefined || request.Recurrence.ValueKind == JsonValueKind.Null) ? null : JsonSerializer.Serialize(request.Recurrence),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userRefId = GetCurrentUserRefId();
            if (userRefId != null) booking.UserRefId = userRefId;

            await _bookingService.CreateAsync(booking, cancellationToken);

            // Build response DTO to avoid serialization cycles
            var response = new
            {
                booking.BookingId,
                booking.RoomId,
                booking.UserRefId,
                booking.Title,
                booking.Description,
                booking.StartDatetime,
                booking.EndDatetime,
                booking.Recurrence,
                booking.Status,
                booking.CreatedAt,
                booking.UpdatedAt
            };

            return CreatedAtAction(nameof(Get), new { id = booking.BookingId }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest();

            var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
            if (booking == null) return NotFound();

            // Only owner can edit while Pending; Admins can edit any
            var userRefId = GetCurrentUserRefId();
            var isAdmin = HttpContext.User?.IsInRole("Admin") == true || HttpContext.User?.IsInRole("SuperAdmin") == true;
            if (!isAdmin)
            {
                if (booking.UserRefId == null || userRefId == null || booking.UserRefId != userRefId) return Forbid();
                if (!string.Equals(booking.Status, "Pending", StringComparison.OrdinalIgnoreCase)) return BadRequest("Only pending bookings can be edited by the user");
            }

            // conflict check similar to create
            var existing = (await _bookingService.GetBookingsAsync(cancellationToken)).Where(b => b.RoomId == request.RoomId && b.BookingId != id).ToList();
            var occurrences = GenerateOccurrences(request, 365).ToList();
            foreach (var occ in occurrences)
            {
                if (HasConflict(existing, occ.start, occ.end))
                {
                    return Conflict(new { message = "Requested time (or recurring series) conflicts with existing bookings" });
                }
            }

            // Get room via RoomService through controller base services
            // Use service locator pattern via HttpContext.RequestServices
            var roomService = HttpContext.RequestServices.GetService(typeof(ASI.Basecode.Services.Interfaces.IRoomService)) as ASI.Basecode.Services.Interfaces.IRoomService;
            if (roomService != null)
            {
                var room = await roomService.GetByIdAsync(request.RoomId, cancellationToken);
                if (room == null) return NotFound(new { message = "Room not found" });

                if (!string.IsNullOrWhiteSpace(room.OperatingHours))
                {
                    RoomOperatingHoursDto ops = null;
                    try { ops = JsonSerializer.Deserialize<RoomOperatingHoursDto>(room.OperatingHours); } catch { ops = null; }

                    if (ops != null)
                    {
                        foreach (var occ in occurrences)
                        {
                            var day = occ.start.DayOfWeek;
                            DayHoursDto dayHours = (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) ? ops.Weekends : ops.Weekdays;
                            if (dayHours == null || string.IsNullOrWhiteSpace(dayHours.Open) || string.IsNullOrWhiteSpace(dayHours.Close))
                            {
                                return BadRequest(new { message = "Room operating hours are not configured for this day" });
                            }

                            if (!TimeSpan.TryParse(dayHours.Open, CultureInfo.InvariantCulture, out var openTs) || !TimeSpan.TryParse(dayHours.Close, CultureInfo.InvariantCulture, out var closeTs))
                            {
                                return BadRequest(new { message = "Room operating hours time format is invalid" });
                            }

                            var occStartTs = occ.start.TimeOfDay;
                            var occEndTs = occ.end.TimeOfDay;

                            if (occStartTs < openTs || occEndTs > closeTs)
                            {
                                return BadRequest(new { message = $"Requested time {occ.start} - {occ.end} is outside room operating hours ({dayHours.Open} - {dayHours.Close})" });
                            }
                        }
                    }
                }
            }

            // apply updates
            booking.RoomId = request.RoomId;
            booking.Title = request.Title;
            booking.Description = request.Description;
            booking.StartDatetime = request.StartDatetime;
            booking.EndDatetime = request.EndDatetime;
            booking.Recurrence = (request.Recurrence.ValueKind == JsonValueKind.Undefined || request.Recurrence.ValueKind == JsonValueKind.Null) ? null : JsonSerializer.Serialize(request.Recurrence);
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingService.UpdateAsync(booking, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
            if (booking == null) return NotFound();

            var userRefId = GetCurrentUserRefId();
            var isAdmin = HttpContext.User?.IsInRole("Admin") == true || HttpContext.User?.IsInRole("SuperAdmin") == true;

            if (!isAdmin)
            {
                if (booking.UserRefId == null || userRefId == null || booking.UserRefId != userRefId) return Forbid();
                if (!string.Equals(booking.Status, "Pending", StringComparison.OrdinalIgnoreCase)) return BadRequest("Only pending bookings can be cancelled by the user");
            }

            await _bookingService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        // Admin endpoints
        [HttpPost("Approve/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
            if (booking == null) return NotFound();
            booking.Status = "Approved";
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingService.UpdateAsync(booking, cancellationToken);
            return NoContent();
        }

        [HttpPost("Decline/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Decline(int id, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
            if (booking == null) return NotFound();
            booking.Status = "Declined";
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingService.UpdateAsync(booking, cancellationToken);
            return NoContent();
        }
    }
}
