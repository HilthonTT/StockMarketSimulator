namespace Contracts.Emails;

public sealed record EmailVerificationEmail(string EmailTo, string VerificationLink);
