using ASI.Basecode.Resources.Constants;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp
{
    // Authorization configuration
    internal partial class StartupConfigurer
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly TokenProviderOptions _tokenProviderOptions;

        /// <summary>
        /// Configure authorization
        /// </summary>
        private void ConfigureAuthorization()
        {
            var token = Configuration.GetTokenAuthentication();
            var tokenProviderOptionsFactory = this._services.BuildServiceProvider().GetService<TokenProviderOptionsFactory>();
            var tokenValidationParametersFactory = this._services.BuildServiceProvider().GetService<TokenValidationParametersFactory>();
            var tokenValidationParameters = tokenValidationParametersFactory.Create();

            // Use JWT bearer as the default authentication scheme so Authorization: Bearer <token> works
            this._services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                // keep cookie scheme available for interactive sign-in
                options.DefaultSignInScheme = Const.AuthenticationScheme;
            })
            .AddJwtBearer(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            })
            .AddCookie(Const.AuthenticationScheme, options =>
            {
                options.Cookie = new CookieBuilder()
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    Name = $"{this._environment.ApplicationName}_{token.CookieName}"
                };
                options.LoginPath = new PathString("/Account/Login");
                options.AccessDeniedPath = new PathString("/html/Forbidden.html");
                options.ReturnUrlParameter = "ReturnUrl";
                options.TicketDataFormat = new CustomJwtDataFormat(SecurityAlgorithms.HmacSha256, _tokenValidationParameters, Configuration, tokenProviderOptionsFactory);

                // Prevent automatic redirects for API calls; return status codes instead
                options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (IsApiRequest(ctx.Request))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        if (IsApiRequest(ctx.Request))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.CompletedTask;
                    }
                };
            });

            this._services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                // Admin policy that accepts either Admin or SuperAdmin roles
                options.AddPolicy("RequireAdmin", policy =>
                {
                    policy.RequireRole("Admin", "SuperAdmin");
                });
            });

            this._services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter("RequireAuthenticatedUser"));
            });
        }

        private static bool IsApiRequest(HttpRequest request)
        {
            return request.Path.StartsWithSegments(new PathString("/api"));
        }
    }
}
