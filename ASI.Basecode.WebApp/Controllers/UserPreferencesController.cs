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
        public IActionResult GetByUser(string userId)
        {
            var items = _prefService.GetByUser(userId).ToList();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _prefService.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserPreference model)
        {
            if (model == null) return BadRequest();
            _prefService.Create(model);
            return CreatedAtAction(nameof(Get), new { id = model.PrefId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] UserPreference model)
        {
            if (model == null || model.PrefId != id) return BadRequest();
            _prefService.Update(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _prefService.Delete(id);
            return NoContent();
        }
    }
}
