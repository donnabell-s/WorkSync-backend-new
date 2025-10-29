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
    public class UserPreferencesController : ASI.Basecode.WebApp.Mvc.ControllerBase<UserPreferencesController>
    {
        private readonly IUserPreferenceService _prefService;

        public UserPreferencesController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserPreferenceService prefService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _prefService = prefService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken)
        {
            var items = await _prefService.GetByUserAsync(userId, cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _prefService.GetByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPreference model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _prefService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.PrefId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserPreference model, CancellationToken cancellationToken)
        {
            if (model == null || model.PrefId != id) return BadRequest();
            await _prefService.UpdateAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _prefService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
