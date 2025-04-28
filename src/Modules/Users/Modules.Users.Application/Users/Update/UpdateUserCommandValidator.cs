using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.Update;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(ValidationErrors.UpdateUser.UserIdIsRequired);

        RuleFor(x => x.Username)
            .NotEmpty().WithError(ValidationErrors.UpdateUser.UsernameIsRequired);
    }
}
