using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.GetTopPerfomer;

internal sealed class GetTopPerformerQueryValidator : AbstractValidator<GetTopPerformerQuery>
{
    public GetTopPerformerQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(StocksValidationErrors.GetTopPerfomer.UserIdIsRequired);
    }
}
