namespace Infrastructure.Emails.Options;

internal sealed class EmailOptions
{
    public const string SettingsKey = "Email";

    public string SenderDisplayName { get; init; } = string.Empty;

    public string SenderEmail { get; init; } = string.Empty;

    public string SmtpPassword { get; init; } = string.Empty;

    public string SmtpServer { get; init; } = string.Empty;

    public int SmtpPort { get; init; }

    /// <summary>Whether SMTP credentials are present. Without them emails are logged, not sent.</summary>
    public bool IsSmtpConfigured =>
        !string.IsNullOrWhiteSpace(SenderEmail) && !string.IsNullOrWhiteSpace(SmtpPassword);
}
