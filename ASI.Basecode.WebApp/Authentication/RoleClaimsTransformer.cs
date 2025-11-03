using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;

namespace ASI.Basecode.WebApp.Authentication
{
    /// <summary>
    /// Normalizes incoming role claim values to TitleCase and adds them as ClaimTypes.Role so Authorize(Roles=...) works consistently.
    /// This handles tokens that emit roles as "role", "roles", or the schema urn claim types and with different casing.
    /// </summary>
    public class RoleClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal == null) return Task.FromResult<ClaimsPrincipal>(null);

            var roleClaims = principal.Claims
                .Where(c => string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase)
                            || c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!roleClaims.Any()) return Task.FromResult(principal);

            var transformIdentity = new ClaimsIdentity();

            foreach (var rc in roleClaims)
            {
                if (string.IsNullOrWhiteSpace(rc.Value)) continue;
                // Normalize to Title Case (e.g., "superadmin" -> "Superadmin" -> then capitalize properly to SuperAdmin)
                var lower = rc.Value.ToLowerInvariant();
                // Handle common compound names like superadmin -> SuperAdmin
                var parts = lower.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(parts[i]);
                }
                var normalized = string.Join("", parts);

                transformIdentity.AddClaim(new Claim(ClaimTypes.Role, normalized));
            }

            principal.AddIdentity(transformIdentity);
            return Task.FromResult(principal);
        }
    }
}
