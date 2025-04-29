using System.Net.Mail;
using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Email
{
    public const int MaxLength = 320;

    private Email() { } // Required by EF Core

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string? email) =>
        Result.Create(email, EmailErrors.Empty)
            .Ensure(e => !string.IsNullOrWhiteSpace(e), EmailErrors.Empty)
            .Ensure(e => e.Length <= MaxLength, EmailErrors.TooLong)
            .Ensure(IsValidEmailFormat, EmailErrors.InvalidFormat)
            .Map(e => new Email(e));

    private static bool IsValidEmailFormat(string? email)
    {
        try
        {
            var mailAddress = new MailAddress(email ?? string.Empty);

            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
