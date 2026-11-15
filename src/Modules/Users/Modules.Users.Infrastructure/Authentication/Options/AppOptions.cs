namespace Modules.Users.Infrastructure.Authentication.Options;

/// <summary>
/// Public-facing application settings.
/// </summary>
internal sealed class AppOptions
{
    public const string SettingsKey = "App";

    /// <summary>
    /// The absolute base URL users reach the API at (e.g. <c>https://api.example.com</c>). Links in emails
    /// are built from it instead of the request's Host header, which a caller controls.
    /// </summary>
    public string PublicApiUrl { get; init; } = string.Empty;
}
