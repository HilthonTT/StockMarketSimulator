namespace Contracts.Emails;

public sealed record MailRequest(string EmailTo, string Subject, string Body);
