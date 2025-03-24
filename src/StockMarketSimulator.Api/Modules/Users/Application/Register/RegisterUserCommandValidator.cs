using FluentValidation;
using StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(512).WithMessage("Password is at most 512 characters long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(Password.MinLength).WithMessage($"Password must be at least {Password.MinLength} characters long")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one digit")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .Matches(x => x.Password).WithMessage("The passwords do not match");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
             .MaximumLength(256).WithMessage("Username is at most 256 characters long");
    }
}
