using FluentValidation;

namespace Modules.Users.BackgroundJobs.Users.UserEmailVerified;

public sealed class UserEmailVerifiedJobDataValidator : AbstractValidator<UserEmailVerifiedJobData>
{
    public UserEmailVerifiedJobDataValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Username).NotEmpty();
    }
}
