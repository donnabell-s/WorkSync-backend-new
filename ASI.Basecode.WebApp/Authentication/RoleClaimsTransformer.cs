using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;

namespace ASI.Basecode.WebApp.Authentication
{
    /// <summary>
    /// Normalizes incoming role claim values to canonical role names and adds them as ClaimTypes.Role so Authorize(Roles=...) works consistently.
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
                // Normalize to Title Case parts (e.g., "super-admin" -> ["Super","Admin"]) and join
                var lower = rc.Value.ToLowerInvariant();
                var parts = lower.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(p))
                                 .ToArray();

                var normalized = string.Join("", parts);

                // Map known special cases to canonical role names
                if (string.Equals(normalized, "Superadmin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(lower, "superadmin", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = "SuperAdmin"; // canonical format used in Authorize attributes
                }
                else if (string.Equals(normalized, "Superuser", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(lower, "super-user", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = "SuperAdmin";
                }
                // Other common normalization: allow 'admin' as 'Admin'
                else if (string.Equals(normalized, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(lower, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = "Admin";
                }

                // Add normalized role claim
                transformIdentity.AddClaim(new Claim(ClaimTypes.Role, normalized));

                // Also add the original role value as a ClaimTypes.Role to maximize compatibility
                if (!string.Equals(rc.Value, normalized, StringComparison.Ordinal))
                {
                    transformIdentity.AddClaim(new Claim(ClaimTypes.Role, rc.Value));
                }
            }

            principal.AddIdentity(transformIdentity);
            return Task.FromResult(principal);
        }
    }
}
