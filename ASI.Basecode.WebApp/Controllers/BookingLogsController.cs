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
        public IActionResult Get()
        {
            var items = _logService.GetBookingLogs().ToList();
            return Ok(items);
        }

        [HttpGet("booking/{bookingId}")]
        public IActionResult GetByBooking(int bookingId)
        {
            var items = _logService.GetByBookingId(bookingId).ToList();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _logService.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Post([FromBody] BookingLog model)
        {
            if (model == null) return BadRequest();
            _logService.Create(model);
            return CreatedAtAction(nameof(Get), new { id = model.BookingLogId }, model);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logService.Delete(id);
            return NoContent();
        }
    }
}
