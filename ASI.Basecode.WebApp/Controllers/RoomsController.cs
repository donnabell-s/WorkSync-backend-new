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
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _roomService.GetRoomsAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var item = await _roomService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Room model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _roomService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.RoomId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Room model, CancellationToken cancellationToken)
        {
            if (model == null || model.RoomId != id) return BadRequest();
            await _roomService.UpdateAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            await _roomService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
