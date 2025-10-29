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
        public IActionResult Get()
        {
            var items = _sessionService.GetSessions().ToList();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var item = _sessionService.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Session model)
        {
            if (model == null) return BadRequest();
            _sessionService.Create(model);
            return CreatedAtAction(nameof(Get), new { id = model.SessionId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Session model)
        {
            if (model == null || model.SessionId != id) return BadRequest();
            _sessionService.Update(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _sessionService.Delete(id);
            return NoContent();
        }
    }
}
