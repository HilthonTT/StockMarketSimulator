using System.Security.Claims;
using SharedKernel;

namespace Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new UnauthorizedException("User id is unavailable");
    }

    public static bool TryGetUserId(this ClaimsPrincipal? principal, out Guid parsedUserId)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out parsedUserId);
    }
}
