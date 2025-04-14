using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Users;

namespace Modules.Users.Infrastructure.Authentication;

internal sealed class EmailVerificationLinkFactory(
    IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator) : IEmailVerificationLinkFactory
{
    public string Create(Guid emailVerificationTokenId)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return $"https://test-host/verify-email?tokenId={emailVerificationTokenId}";
        }

        string? verificationLink = linkGenerator.GetUriByName(
            httpContext,
            UserEndpoints.VerifyEmail,
            new { token = emailVerificationTokenId });

        return verificationLink ?? throw new NullReferenceException("Could not create email verification link");
    }
}
