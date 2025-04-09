using FluentValidation;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

internal sealed class GetTransactionsByUserIdQueryValidator : AbstractValidator<GetTransactionsByUserIdQuery>
{
    public GetTransactionsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
