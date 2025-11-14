using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
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
            _sessionManager = new SessionManager(this._session);
            _signInManager = signInManager;
            _tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            _tokenValidationParametersFactory = tokenValidationParametersFactory;
            _appConfiguration = configuration;
            _userService = userService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            User user = null;
            var result = _userService.AuthenticateUserByEmail(model.Email, model.Password, ref user);

            if (result != ASI.Basecode.Resources.Constants.Enums.LoginResult.Success || user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var identity = _signInManager.CreateClaimsIdentity(user);

            // Ensure we have full user properties (FirstName/LastName) from repository in case Authenticate returned a lightweight object
            try
            {
                User fullUser = null;
                if (!string.IsNullOrWhiteSpace(user?.Email))
                {
                    fullUser = _userRepository.GetByEmail(user.Email);
                }
                // Fall back to id lookup if email lookup not possible
                if (fullUser == null && user != null)
                {
                    fullUser = _userRepository.GetById(user.Id);
                }

                if (fullUser != null)
                {
                    user = fullUser;
                }
            }
            catch
            {
                // ignore and continue with whatever data we have on user
            }

            // Read token authentication settings directly from configuration section
            // Read token authentication values directly via configuration keys to avoid extension method ambiguity
            var tokenConfig = new ASI.Basecode.WebApp.Models.TokenAuthentication
            {
                SecretKey = _appConfiguration["TokenAuthentication:SecretKey"],
                Audience = _appConfiguration["TokenAuthentication:Audience"],
                TokenPath = _appConfiguration["TokenAuthentication:TokenPath"],
                CookieName = _appConfiguration["TokenAuthentication:CookieName"],
                ExpirationMinutes = int.TryParse(_appConfiguration["TokenAuthentication:ExpirationMinutes"], out var mins) ? mins : 60
            };
            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(tokenConfig.SecretKey));
            var tokenOptions = TokenProviderOptionsFactory.Create(tokenConfig, signingKey);

            var tokenProvider = new TokenProvider(Options.Create(tokenOptions));
            var encodedJwt = tokenProvider.GetJwtSecurityToken(identity, tokenOptions);

            _session.SetString("HasSession", "Exist");
            _session.SetString("UserName", user.Email ?? string.Empty);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)tokenOptions.Expiration.TotalSeconds,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    role = user.Role,
                    firstName = user.FirstName ?? string.Empty,
                    lastName = user.LastName ?? string.Empty
                }
            };

            return Ok(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = _userRepository.GetByEmail(model.Email);
            if (existing != null) return Conflict(new { message = "User already exists." });

            var newUser = new User
            {
                Email = model.Email,
                UserId = System.Guid.NewGuid().ToString(),
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

        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult GetUsers()
        {
            // Materialize users server-side first then perform case-insensitive role-part matching in-memory
            var allUsers = _userRepository.GetUsers().AsEnumerable();

            var users = allUsers
                .Where(u =>
                {
                    if (string.IsNullOrWhiteSpace(u.Role)) return false;
                    // split on common delimiters and check any role equals 'user'
                    var parts = u.Role.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Any(p => string.Equals(p.Trim(), "user", StringComparison.OrdinalIgnoreCase));
                })
                .Select(u => new
                {
                    u.Id,
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

        [HttpGet]
        [Authorize(Policy = "RequireSuperAdmin")]
        public IActionResult GetAdmins()
        {
            var admins = _userRepository.GetUsers().AsEnumerable()
                .Where(u => u.Role != null && u.Role.ToLower().Contains("admin") && !u.Role.ToLower().Contains("super"))
                .Select(u => new
                {
                    u.Id,
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

            if ((requestedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || requestedRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)) && !callerIsSuperAdmin)
            {
                return Forbid();
            }

            var newUser = new User
            {
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

        [HttpPut("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto model)
        {
            if (model == null) return BadRequest();

            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();

            var callerIsSuperAdmin = HttpContext.User?.IsInRole("SuperAdmin") == true;

            if (!callerIsSuperAdmin && user.Role != null && (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || user.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(model.Role) && (model.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || model.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)) && !callerIsSuperAdmin)
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(model.FirstName)) user.FirstName = model.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(model.LastName)) user.LastName = model.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var existing = _userRepository.GetByEmail(model.Email);
                if (existing != null && existing.Id != user.Id) return Conflict(new { message = "Email already in use by another user." });
                user.Email = model.Email;
            }

            if (!string.IsNullOrWhiteSpace(model.Password)) user.PasswordHash = PasswordManager.EncryptPassword(model.Password);

            if (model.IsActive.HasValue) user.IsActive = model.IsActive.Value;
            if (!string.IsNullOrWhiteSpace(model.Role)) user.Role = model.Role.Trim();

            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();

            var callerIsSuperAdmin = HttpContext.User?.IsInRole("SuperAdmin") == true;

            if (!callerIsSuperAdmin && user.Role != null && (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || user.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return Forbid();
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
