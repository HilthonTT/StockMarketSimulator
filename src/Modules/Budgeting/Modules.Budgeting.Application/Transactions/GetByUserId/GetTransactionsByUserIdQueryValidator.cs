using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

internal sealed class GetTransactionsByUserIdQueryValidator : AbstractValidator<GetTransactionsByUserIdQuery>
{
    public GetTransactionsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(ValidationErrors.GetTransactionsByUserId.UserIdIsRequired);
    }
}
