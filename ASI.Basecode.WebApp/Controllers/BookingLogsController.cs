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

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
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
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _logService.GetBookingLogsAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBooking(int bookingId, CancellationToken cancellationToken)
        {
            var items = await _logService.GetByBookingIdAsync(bookingId, cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _logService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
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
