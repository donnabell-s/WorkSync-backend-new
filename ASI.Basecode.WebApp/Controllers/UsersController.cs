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
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var users = await _userAdminService.GetUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var user = await _userAdminService.GetUserByIdAsync(id, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Create user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            await _userAdminService.CreateUserAsync(model, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User model, CancellationToken cancellationToken)
        {
            if (model == null || model.Id != id) return BadRequest();
            await _userAdminService.UpdateUserAsync(model, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _userAdminService.DeleteUserAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
