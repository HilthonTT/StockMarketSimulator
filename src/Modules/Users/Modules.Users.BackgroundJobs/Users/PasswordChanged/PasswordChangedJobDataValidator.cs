using FluentValidation;

namespace Modules.Users.BackgroundJobs.Users.PasswordChanged;

internal sealed class PasswordChangedJobDataValidator : AbstractValidator<PasswordChangedJobData>
{
    public PasswordChangedJobDataValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
