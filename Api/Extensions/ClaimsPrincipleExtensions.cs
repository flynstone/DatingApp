using System.Security.Claims;

namespace Api.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        // Easy accessor extension.
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
