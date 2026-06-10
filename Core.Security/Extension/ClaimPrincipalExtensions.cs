using System.Collections.Immutable;
using System.Security.Claims;

namespace Core.Security.Extension
{
    public static class ClaimPrincipalExtensions
    {
        public static ICollection<string>? GetRoleClaims(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal?.GetClaims(ClaimTypes.Role);
        }

        public static ICollection<string>? GetClaims(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            return claimsPrincipal?.FindAll(claimType)?.Select(c => c.Value).ToImmutableArray();
        }
    }
}