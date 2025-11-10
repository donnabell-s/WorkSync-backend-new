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
    [Route("api/[controller]")]
    public class AdminsController : ASI.Basecode.WebApp.Mvc.ControllerBase<AdminsController>
    {
        private readonly IUserAdminService _userAdminService;

        public AdminsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserAdminService userAdminService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userAdminService = userAdminService;
        }

        /// <summary>
        /// Get all admins
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var admins = await _userAdminService.GetAdminsAsync(cancellationToken);
            return Ok(admins);
        }

        /// <summary>
        /// Get admin by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var admin = await _userAdminService.GetAdminByIdAsync(id, cancellationToken);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        /// <summary>
        /// Create admin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _userAdminService.CreateAdminAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update admin
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User model, CancellationToken cancellationToken)
        {
            if (model == null || model.Id != id) return BadRequest();
            await _userAdminService.UpdateAdminAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete admin (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _userAdminService.DeleteAdminAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
