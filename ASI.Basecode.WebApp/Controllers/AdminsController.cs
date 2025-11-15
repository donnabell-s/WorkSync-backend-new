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
        /// Add admin (SuperAdmin only)
        /// </summary>
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] User model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can add admins.");
            }

            if (model == null) return BadRequest();
            await _userAdminService.CreateAdminAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>
        /// Edit admin (SuperAdmin only)
        /// </summary>
        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] User model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can edit admins.");
            }

            if (model == null || model.Id != id) return BadRequest();
            await _userAdminService.UpdateAdminAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete admin (SuperAdmin only)
        /// </summary>
        [HttpDelete("Delete/{id}")]
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
