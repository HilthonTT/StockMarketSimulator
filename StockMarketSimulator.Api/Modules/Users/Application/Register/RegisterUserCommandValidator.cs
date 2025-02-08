using FluentValidation;

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
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");

        RuleFor(x => x.ConfirmPassword)
            .Matches(x => x.Password).WithMessage("The passwords do not match");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
             .MaximumLength(256).WithMessage("Username is at most 256 characters long");
    }
}
