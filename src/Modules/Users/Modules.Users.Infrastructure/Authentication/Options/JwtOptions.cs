namespace Modules.Users.Infrastructure.Authentication.Options;

internal sealed class JwtOptions
{
    public const string SettingsKey = "Jwt";

    public string Secret { get; init; } = string.Empty;

    public int ExpirationInMinutes { get; init; }

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;
}
