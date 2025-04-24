using FluentValidation;

namespace Modules.Users.BackgroundJobs.Users.EmailVerification;

internal sealed class EmailVerificationJobDataValidator : AbstractValidator<EmailVerificationJobData>
{
    public EmailVerificationJobDataValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.VerificationLink).NotEmpty();
    }
}
