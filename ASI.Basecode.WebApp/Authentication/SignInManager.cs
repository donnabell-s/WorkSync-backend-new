using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Authentication
{
    /// <summary>
    /// SignInManager
    /// </summary>
    public class SignInManager
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public LoginUser user { get; set; }

        /// <summary>
        /// Initializes a new instance of the SignInManager class.
        /// </summary>
        public SignInManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SignInManager class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="userService">The user service.</param>
        public SignInManager(IConfiguration configuration,
                             IHttpContextAccessor httpContextAccessor,
                             IUserService userService)
        {
            this._configuration = configuration;
            this._httpContextAccessor = httpContextAccessor;
            this._userService = userService;
            user = new LoginUser();
        }

        /// <summary>
        /// Gets the claims identity.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The successfully completed task</returns>
        public Task<ClaimsIdentity> GetClaimsIdentity(string username, string password)
        {
            ClaimsIdentity claimsIdentity = null;
            User userData = new User();

            if (this._userService != null)
            {
                user.loginResult = this._userService.AuthenticateUser(username, password, ref userData);
            }
            else
            {
                // Fallback to previous stub behavior
                user.loginResult = LoginResult.Success; // TODO implement real authentication
            }

            if (user.loginResult == LoginResult.Failed)
            {
                return Task.FromResult<ClaimsIdentity>(null);
            }

            user.userData = userData;
            claimsIdentity = CreateClaimsIdentity(userData);
            return Task.FromResult(claimsIdentity);
        }

        /// <summary>
        /// Creates the claims identity.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Instance of ClaimsIdentity</returns>
        public ClaimsIdentity CreateClaimsIdentity(User user)
        {
            var token = _configuration.GetTokenAuthentication();

            // Build a safe display name: prefer first+last name, fallback to email, then userId
            var displayName = string.Empty;
            if (!string.IsNullOrWhiteSpace(user?.FirstName) || !string.IsNullOrWhiteSpace(user?.LastName))
            {
                displayName = $"{user?.FirstName?.Trim()} {user?.LastName?.Trim()}".Trim();
            }
            else if (!string.IsNullOrWhiteSpace(user?.Email))
            {
                displayName = user.Email;
            }
            else
            {
                displayName = user?.UserId ?? string.Empty;
            }

            var claims = new List<Claim>()
            {
                // Use numeric DB Id as the NameIdentifier so middleware and controllers can locate numeric user id easily
                new Claim(ClaimTypes.NameIdentifier, user?.Id.ToString() ?? string.Empty, ClaimValueTypes.Integer, Const.Issuer),
                new Claim(ClaimTypes.Name, displayName, ClaimValueTypes.String, Const.Issuer),

                // Preserve the original textual UserId (username/email) in a separate claim
                new Claim("UserId", user.UserId ?? string.Empty, ClaimValueTypes.String, Const.Issuer),
                new Claim("UserName", displayName, ClaimValueTypes.String, Const.Issuer),

                // Add role claims so JWT contains role information (both ClaimTypes.Role and "role" for compatibility)
                new Claim(ClaimTypes.Role, user?.Role ?? "user", ClaimValueTypes.String, Const.Issuer),
                new Claim("role", user?.Role ?? "user", ClaimValueTypes.String, Const.Issuer),
            };

            // Add numeric UserRefId claim if available (this is the DB numeric Id)
            if (user?.Id != null)
            {
                claims.Add(new Claim("UserRefId", user.Id.ToString(), ClaimValueTypes.Integer, Const.Issuer));
            }

            return new ClaimsIdentity(claims, Const.AuthenticationScheme);
        }

        /// <summary>
        /// Creates the claims principal.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns>Created claims principal</returns>
        public IPrincipal CreateClaimsPrincipal(ClaimsIdentity identity)
        {
            var identities = new List<ClaimsIdentity>();
            identities.Add(identity);
            return this.CreateClaimsPrincipal(identities);
        }

        /// <summary>
        /// Creates the claims principal.
        /// </summary>
        /// <param name="identities">The identities.</param>
        /// <returns>Created claims principal</returns>
        public IPrincipal CreateClaimsPrincipal(IEnumerable<ClaimsIdentity> identities)
        {
            var principal = new ClaimsPrincipal(identities);
            return principal;
        }

        /// <summary>
        /// Signs in user asynchronously
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="isPersistent">if set to <c>true</c> [is persistent].</param>
        public async Task SignInAsync(User user, bool isPersistent = false)
        {
            var claimsIdentity = this.CreateClaimsIdentity(user);
            var principal = this.CreateClaimsPrincipal(claimsIdentity);
            await this.SignInAsync(principal, isPersistent);
        }

        /// <summary>
        /// Signs in user asynchronously
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="isPersistent">if set to <c>true</c> [is persistent].</param>
        public async Task SignInAsync(IPrincipal principal, bool isPersistent = false)
        {
            var token = _configuration.GetTokenAuthentication();
            await _httpContextAccessor
                .HttpContext
                .SignInAsync(
                            Const.AuthenticationScheme,
                            (ClaimsPrincipal)principal,
                            new AuthenticationProperties
                            {
                                ExpiresUtc = DateTime.UtcNow.AddMinutes(token.ExpirationMinutes),
                                IsPersistent = isPersistent,
                                AllowRefresh = false
                            });
        }

        /// <summary>
        /// Signs out user asynchronously
        /// </summary>
        public async Task SignOutAsync()
        {
            var token = _configuration.GetTokenAuthentication();
            await _httpContextAccessor.HttpContext.SignOutAsync(Const.AuthenticationScheme);
        }
    }
}
