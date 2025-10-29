using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoMapper;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult Get()
        {
            var items = _bookingService.GetBookings().ToList();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _bookingService.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Booking model)
        {
            if (model == null) return BadRequest();
            _bookingService.Create(model);
            return CreatedAtAction(nameof(Get), new { id = model.BookingId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Booking model)
        {
            if (model == null || model.BookingId != id) return BadRequest();
            _bookingService.Update(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _bookingService.Delete(id);
            return NoContent();
        }
    }
}
