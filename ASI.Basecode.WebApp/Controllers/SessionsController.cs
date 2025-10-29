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
    public class SessionsController : ASI.Basecode.WebApp.Mvc.ControllerBase<SessionsController>
    {
        private readonly ISessionService _sessionService;

        public SessionsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ISessionService sessionService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _sessionService = sessionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var items = await _sessionService.GetSessionsAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var item = await _sessionService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Session model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _sessionService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.SessionId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Session model, CancellationToken cancellationToken)
        {
            if (model == null || model.SessionId != id) return BadRequest();
            await _sessionService.UpdateAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            await _sessionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
