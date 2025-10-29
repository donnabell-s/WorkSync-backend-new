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

        [HttpGet]
        public IActionResult Get()
        {
            var items = _roomService.GetRooms().ToList();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var item = _roomService.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Room model)
        {
            if (model == null) return BadRequest();
            _roomService.Create(model);
            return CreatedAtAction(nameof(Get), new { id = model.RoomId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Room model)
        {
            if (model == null || model.RoomId != id) return BadRequest();
            _roomService.Update(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _roomService.Delete(id);
            return NoContent();
        }
    }
}
