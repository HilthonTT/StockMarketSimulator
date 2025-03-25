namespace Contracts.Emails;

public sealed record PasswordChangedEmail(string EmailTo, DateTime Date);
