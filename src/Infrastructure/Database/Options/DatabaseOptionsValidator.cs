using FluentValidation;

namespace Infrastructure.Database.Options;

internal sealed class DatabaseOptionsValidator : AbstractValidator<DatabaseOptions>
{
    public DatabaseOptionsValidator()
    {
        RuleFor(x => x.MaxRetryCount).NotEmpty();

        RuleFor(x => x.CommandTimeout).NotEmpty();

        RuleFor(x => x.EnableDetailedErrors).NotEmpty();

        RuleFor(x => x.EnableSensitiveDataLogging).NotEmpty();
    }
}
