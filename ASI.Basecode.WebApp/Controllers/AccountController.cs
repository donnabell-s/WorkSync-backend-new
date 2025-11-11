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
using System.Linq;
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
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
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

        // ChangeRole endpoint removed temporarily because it's not working. Re-add when ready.

        // GET: api/Account/GetUsers
        // Admins and SuperAdmins may call this to list users
        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers()
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/Account/GetAdmins
        // Only SuperAdmin may call this to list admins (Admin and SuperAdmin accounts)
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetAdmins()
        {
            var admins = _userRepository.GetUsers()
                .Where(u => u.Role != null && (u.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || u.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToList();

            return Ok(admins);
        }

        // POST: api/Account/CreateUser
        // Admins and SuperAdmins can create users. Creating Admin/SuperAdmin requires caller to be SuperAdmin.
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult CreateUser([FromBody] CreateUserDto model)
        {
            if (model == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Email and Password are required." });

            var existing = _userRepository.GetByEmail(model.Email);
            if (existing != null) return Conflict(new { message = "User already exists." });

            var callerIsSuperAdmin = HttpContext.User?.IsInRole("SuperAdmin") == true;

            var requestedRole = string.IsNullOrWhiteSpace(model.Role) ? "user" : model.Role.Trim();

            if ((requestedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || requestedRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                && !callerIsSuperAdmin)
            {
                return Forbid(); // only SuperAdmin may create admin/superadmin accounts
            }

            var newUser = new User
            {
                UserId = model.Email,
                Email = model.Email,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                PasswordHash = PasswordManager.EncryptPassword(model.Password),
                Role = requestedRole,
                IsActive = model.IsActive ?? true,
                CreatedAt = DateTime.UtcNow
            };

            _userRepository.Add(newUser);
            _unitOfWork.SaveChanges();

            var result = new { id = newUser.Id, email = newUser.Email, role = newUser.Role, isActive = newUser.IsActive };
            return CreatedAtAction(nameof(GetUsers), result);
        }

        // PUT: api/Account/UpdateUser/{id}
        // Admins and SuperAdmins may update users; changing admin/superadmin roles requires SuperAdmin
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto model)
        {
            if (model == null) return BadRequest();

            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();

            var callerIsSuperAdmin = HttpContext.User?.IsInRole("SuperAdmin") == true;

            // If the target user is an admin/superadmin and caller is not superadmin => forbid any change to that admin
            if (!callerIsSuperAdmin && user.Role != null && (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || user.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return Forbid();
            }

            // If model requests role change to admin/superadmin, require caller superadmin
            if (!string.IsNullOrWhiteSpace(model.Role) &&
                (model.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || model.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)) &&
                !callerIsSuperAdmin)
            {
                return Forbid();
            }

            // Apply updates
            if (!string.IsNullOrWhiteSpace(model.FirstName)) user.FirstName = model.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(model.LastName)) user.LastName = model.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var existing = _userRepository.GetByEmail(model.Email);
                if (existing != null && existing.Id != user.Id) return Conflict(new { message = "Email already in use by another user." });
                user.Email = model.Email;
                user.UserId = model.Email;
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = PasswordManager.EncryptPassword(model.Password);
            }

            if (model.IsActive.HasValue) user.IsActive = model.IsActive.Value;
            if (!string.IsNullOrWhiteSpace(model.Role)) user.Role = model.Role.Trim();

            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE: api/Account/DeleteUser/{id}
        // Admins and SuperAdmins can delete users. Deleting Admin or SuperAdmin accounts is restricted to SuperAdmin only.
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();

            var callerIsSuperAdmin = HttpContext.User?.IsInRole("SuperAdmin") == true;

            if (!callerIsSuperAdmin && user.Role != null &&
                (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || user.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return Forbid(); // only superadmin can delete admin/superadmin accounts
            }

            _userRepository.Delete(user);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

    }

    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool? IsActive { get; set; }
        public string Password { get; set; }
    }
}
