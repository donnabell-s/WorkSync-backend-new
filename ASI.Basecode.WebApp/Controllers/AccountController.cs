using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="accountService">The account service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            IAccountService accountService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._accountService = accountService;
        }

        /// <summary>
        /// Login Method
        /// </summary>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)

        {
            this._session.SetString("HasSession", "Exist");

            User user = null;

            //await this._signInManager.SignInAsync(user);
            this._session.SetString("UserName", model.UserId);

            return Ok(user);
        }

        /// <summary>
        /// Sign Out current account
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return Ok();
        }

        /// <summary>
        /// Get user account information
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId, CancellationToken cancellationToken)
        {
            var user = await _accountService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Update user account information
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] User user, CancellationToken cancellationToken)
        {
            if (user == null || user.UserId != userId) return BadRequest();
            await _accountService.UpdateUserAsync(user, cancellationToken);
            return Ok(user);
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password/{userId}")]
        public async Task<IActionResult> ChangePassword(string userId, [FromBody] ChangePasswordModel model, CancellationToken cancellationToken)
        {
            if (model == null) return BadRequest();
            try
            {
                await _accountService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword, cancellationToken);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
