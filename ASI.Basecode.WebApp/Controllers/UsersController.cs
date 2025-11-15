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
    public class UsersController : ASI.Basecode.WebApp.Mvc.ControllerBase<UsersController>
    {
        private readonly IUserAdminService _userAdminService;

        public UsersController(
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
        /// Get all users (SuperAdmin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view all users.");
            }

            var users = await _userAdminService.GetUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can view user details.");
            }

            var user = await _userAdminService.GetUserByIdAsync(id, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Create user (SuperAdmin only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can create users.");
            }

            if (model == null) return BadRequest();
            await _userAdminService.CreateUserAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update user (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User model, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can update users.");
            }

            if (model == null || model.Id != id) return BadRequest();
            await _userAdminService.UpdateUserAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete user (soft delete) (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole?.ToLower() != "superadmin")
            {
                return Forbid("Only SuperAdmin can delete users.");
            }

            await _userAdminService.DeleteUserAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
