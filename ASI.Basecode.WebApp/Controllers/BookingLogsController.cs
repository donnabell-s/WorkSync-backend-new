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
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    public class BookingLogsController : ASI.Basecode.WebApp.Mvc.ControllerBase<BookingLogsController>
    {
        private readonly IBookingLogService _logService;

        public BookingLogsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IBookingLogService logService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _logService = logService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _logService.GetBookingLogsAsync(cancellationToken);
            
            var results = items.Select(log => new
            {
                log.BookingLogId,
                log.BookingId,
                BookingTitle = log.Booking?.Title ?? "N/A",
                RoomName = log.Booking?.Room?.Name ?? "N/A",
                RoomCode = log.Booking?.Room?.Code ?? "N/A",
                Location = log.Booking?.Room?.Location ?? "N/A",
                Capacity = log.Booking?.Room?.Seats ?? 0,
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

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBooking(int bookingId, CancellationToken cancellationToken)
        {
            var items = await _logService.GetByBookingIdAsync(bookingId, cancellationToken);
            
            var results = items.Select(log => new
            {
                log.BookingLogId,
                log.BookingId,
                BookingTitle = log.Booking?.Title ?? "N/A",
                RoomName = log.Booking?.Room?.Name ?? "N/A",
                RoomCode = log.Booking?.Room?.Code ?? "N/A",
                Location = log.Booking?.Room?.Location ?? "N/A",
                Capacity = log.Booking?.Room?.Seats ?? 0,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _logService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();

            var result = new
            {
                item.BookingLogId,
                item.BookingId,
                BookingTitle = item.Booking?.Title ?? "N/A",
                RoomName = item.Booking?.Room?.Name ?? "N/A",
                RoomCode = item.Booking?.Room?.Code ?? "N/A",
                Location = item.Booking?.Room?.Location ?? "N/A",
                Capacity = item.Booking?.Room?.Seats ?? 0,
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BookingLog model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _logService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.BookingLogId }, model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _logService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
