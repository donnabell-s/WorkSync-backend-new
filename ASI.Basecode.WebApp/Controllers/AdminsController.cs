using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        /// Get all admins (SuperAdmin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view all admins.");
            }

            var admins = await _userAdminService.GetAdminsAsync(cancellationToken);
            return Ok(admins);
        }

        /// <summary>
        /// Get admin by ID (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view admin details.");
            }

            var admin = await _userAdminService.GetAdminByIdAsync(id, cancellationToken);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        /// <summary>
        /// Create admin (SuperAdmin only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can create admins.");
            }

            if (model == null) return BadRequest();
            await _userAdminService.CreateAdminAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update admin (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can update admins.");
            }

            if (model == null || model.Id != id) return BadRequest();
            await _userAdminService.UpdateAdminAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete admin (soft delete) (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can delete admins.");
            }

            await _userAdminService.DeleteAdminAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
