using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Email;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.ChangePassword;

internal sealed class ChangeUserPasswordCommandHandler : ICommandHandler<ChangeUserPasswordCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IValidator<ChangeUserPasswordCommand> _validator;

    public ChangeUserPasswordCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserContext userContext,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IValidator<ChangeUserPasswordCommand> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userContext = userContext;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _validator = validator;
    }

    public async Task<Result> Handle(ChangeUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        if (command.UserId != _userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        User? user = await _userRepository.GetByIdAsync(connection, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        bool verified = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!verified)
        {
            return Result.Failure<TokenResponse>(UserErrors.Unauthorized);
        }

        user.PasswordHash = _passwordHasher.Hash(command.NewPassword);

        await _userRepository.UpdateAsync(connection, user, cancellationToken);

        await _emailService.SendEmailAsync(
             user.Email,
             "Your Password Has Been Changed",
             $"Hello {user.Username},\n\n" +
             $"We wanted to let you know that your password was successfully changed on {DateTime.UtcNow:MMMM d, yyyy 'at' HH:mm} (UTC).\n\n" +
             "If you made this change, no further action is needed.\n\n" +
             "If you did not request this change, please reset your password immediately and contact our support team.\n\n" +
             "Best regards,\nYour Support Team",
             cancellationToken);

        return Result.Success();
    }
}
