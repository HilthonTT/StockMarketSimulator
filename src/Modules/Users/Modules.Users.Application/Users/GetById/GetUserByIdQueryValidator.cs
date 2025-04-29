using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.GetById;

internal sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithError(UsersValidationErrors.GetUserById.UserIdIsRequired);
    }
}
