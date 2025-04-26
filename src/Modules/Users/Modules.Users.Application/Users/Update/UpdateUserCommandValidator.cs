using FluentValidation;

namespace Modules.Users.Application.Users.Update;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Username).NotEmpty();
    }
}
