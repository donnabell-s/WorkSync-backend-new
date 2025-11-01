using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using ASI.Basecode.WebApp.Extensions.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory,
                            IUserRepository userRepository,
                            IUnitOfWork unitOfWork) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._userRepository = userRepository;
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Login Method
        /// </summary>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Authenticate against DB using email
            User user = null;
            var result = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);

            if (result != ASI.Basecode.Resources.Constants.Enums.LoginResult.Success || user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Create claims identity (includes role claim from SignInManager)
            var identity = _signInManager.CreateClaimsIdentity(user);

            // Build token provider options and generate JWT
            var tokenConfig = ASI.Basecode.WebApp.Extensions.Configuration.ConfigurationExtensions.GetTokenAuthentication(_appConfiguration);
            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(tokenConfig.SecretKey));
            var tokenOptions = TokenProviderOptionsFactory.Create(tokenConfig, signingKey);

            var tokenProvider = new TokenProvider(Options.Create(tokenOptions));
            var encodedJwt = tokenProvider.GetJwtSecurityToken(identity, tokenOptions);

            // Optionally set session values
            this._session.SetString("HasSession", "Exist");
            this._session.SetString("UserName", user.Email ?? string.Empty);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)tokenOptions.Expiration.TotalSeconds,
                user = new { id = user.Id, email = user.Email, role = user.Role }
            };

            return Ok(response);
        }

        /// <summary>
        /// Register Method
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists by email
            var existing = _userRepository.GetByEmail(model.Email);
            if (existing != null)
            {
                return Conflict(new { message = "User already exists." });
            }

            var newUser = new User
            {
                // keep UserId as username placeholder (set to email)
                UserId = model.Email,
                Email = model.Email,
                Fname = string.IsNullOrWhiteSpace(model.FullName) ? string.Empty : (model.FullName.Contains(' ') ? model.FullName.Substring(0, model.FullName.IndexOf(' ')) : model.FullName),
                Lname = string.IsNullOrWhiteSpace(model.FullName) ? string.Empty : (model.FullName.Contains(' ') ? model.FullName.Substring(model.FullName.IndexOf(' ') + 1) : string.Empty),
                PasswordHash = PasswordManager.EncryptPassword(model.Password),
                Role = "user",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _userRepository.Add(newUser);
            _unitOfWork.SaveChanges();

            var result = new { id = newUser.Id, email = newUser.Email, role = newUser.Role };
            return CreatedAtAction(nameof(Register), result);
        }

        /// <summary>
        /// Sign Out current account
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return Ok();
        }

        /// <summary>
        /// Change the role of a user
        /// This action is restricted to admins and superadmins.
        /// Superadmins have the capability to assign the superadmin role.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,superadmin")]
        public IActionResult ChangeRole([FromBody] ChangeRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get caller role
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(callerRole))
            {
                return Forbid();
            }

            // prevent non-superadmin from assigning superadmin
            if (model.Role == "superadmin" && callerRole != "superadmin")
            {
                return Forbid();
            }

            var user = _userRepository.GetByEmail(model.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.Role = model.Role;
            _userRepository.Update(user);
            _unitOfWork.SaveChanges();

            return Ok(new { id = user.Id, email = user.Email, role = user.Role });
        }
    }
}
