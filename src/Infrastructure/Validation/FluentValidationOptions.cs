using FluentValidation.Results;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Validation;

public sealed class FluentValidationOptions<TOptions>(IServiceProvider serviceProvider, string? name) 
    : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly string? _name = name;

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_name is not null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        Ensure.NotNull(options, nameof(options));

        using IServiceScope scope = serviceProvider.CreateScope();

        IValidator<TOptions> validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        ValidationResult result = validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        string type = options.GetType().Name;
        var errors = new List<string>();

        foreach (ValidationFailure? failure in result.Errors)
        {
            errors.Add($"Validation failed for {type}.{failure.PropertyName} with the error: {failure.ErrorMessage}");
        }

        return ValidateOptionsResult.Fail(errors);
    }
}