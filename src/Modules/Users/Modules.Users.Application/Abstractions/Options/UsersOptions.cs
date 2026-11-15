namespace Modules.Users.Application.Abstractions.Options;

/// <summary>
/// Users module settings (configuration section <c>Users</c>).
/// </summary>
public sealed class UsersOptions
{
    public const string SettingsKey = "Users";

    /// <summary>
    /// Marks new accounts as verified at registration, so a local install without SMTP can sign in right away.
    /// The normal email-verified flow (Registered role, starting budget, welcome email) still runs.
    /// Never enable this in production.
    /// </summary>
    public bool AutoVerifyEmail { get; init; }
}
