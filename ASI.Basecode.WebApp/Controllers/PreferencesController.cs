using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PreferencesController : ASI.Basecode.WebApp.Mvc.ControllerBase<PreferencesController>
    {
        private readonly IPreferencesService _preferencesService;

        public PreferencesController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IPreferencesService preferencesService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _preferencesService = preferencesService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken)
        {
            var items = await _preferencesService.GetPreferencesByUserAsync(userId, cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var item = await _preferencesService.GetPreferenceByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPreference model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _preferencesService.CreatePreferenceAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.PrefId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserPreference model, CancellationToken cancellationToken)
        {
            if (model == null || model.PrefId != id) return BadRequest();
            await _preferencesService.UpdatePreferenceAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _preferencesService.DeletePreferenceAsync(id, cancellationToken);
            return NoContent();
        }
    }
}

