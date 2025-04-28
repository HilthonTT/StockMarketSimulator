using FluentValidation;
using SharedKernel;

namespace Application.Core.Extensions;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error? error)
    {
        Ensure.NotNull(error, nameof(error));
        Ensure.NotNullOrEmpty(error.Code, nameof(error.Code));
        Ensure.NotNullOrEmpty(error.Description, nameof(error.Description));

        return rule.WithErrorCode(error.Code).WithMessage(error.Description);
    }
}
