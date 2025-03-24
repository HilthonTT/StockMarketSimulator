using FluentValidation.Results;
using SharedKernel;

namespace StockMarketSimulator.Api.Infrastructure.Helpers;

public static class ValidationErrorFactory
{
    public static ValidationError CreateValidationError(IEnumerable<ValidationFailure> validationFailures) =>
        new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
