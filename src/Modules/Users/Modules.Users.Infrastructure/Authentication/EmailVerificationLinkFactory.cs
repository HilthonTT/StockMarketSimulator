using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Users;
using Modules.Users.Infrastructure.Authentication.Options;

namespace Modules.Users.Infrastructure.Authentication;

/// <summary>
/// Builds the link sent in verification emails.
/// </summary>
/// <remarks>
/// The scheme and host come from the configured <see cref="AppOptions.PublicApiUrl"/>, never from the
/// incoming request: with a request-derived host, anyone could register with a victim's address and a
/// forged <c>Host</c> header, and the victim's verification token would be sent to the attacker's site.
/// </remarks>
internal sealed class EmailVerificationLinkFactory(
    IOptions<AppOptions> options,
    LinkGenerator linkGenerator) : IEmailVerificationLinkFactory
{
    private const string FallbackPath = "/api/v1/authentication/verify-email";

    public string Create(Guid emailVerificationTokenId)
    {
        string baseUrl = options.Value.PublicApiUrl.TrimEnd('/');

        // The endpoint lives in the versioned group "api/v{version:apiVersion}".
        string? path = linkGenerator.GetPathByName(
            UserEndpoints.VerifyEmail,
            new { version = "1", token = emailVerificationTokenId });

        return !string.IsNullOrEmpty(path)
            ? $"{baseUrl}{path}"
            : $"{baseUrl}{FallbackPath}?token={emailVerificationTokenId}";
    }
}
