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

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _bookingService.GetBookingsAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _bookingService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Booking model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _bookingService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.BookingId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Booking model, CancellationToken cancellationToken)
        {
            if (model == null || model.BookingId != id) return BadRequest();
            await _bookingService.UpdateAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _bookingService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
