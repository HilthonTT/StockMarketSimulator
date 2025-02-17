using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Budgets.Api;
using StockMarketSimulator.Api.Modules.Roles.Api;
using StockMarketSimulator.Api.Modules.Roles.Domain;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
{
    private const decimal DefaultBuyingPower = 5000;

    private readonly IUserRepository _userRepository;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IBudgetsApi _budgetsApi;
    private readonly IRolesApi _rolesApi;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IDbConnectionFactory dbConnectionFactory,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserCommand> validator,
        IBudgetsApi budgetsApi,
        IRolesApi rolesApi)
    {
        _userRepository = userRepository;
        _dbConnectionFactory = dbConnectionFactory;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _budgetsApi = budgetsApi;
        _rolesApi = rolesApi;
    }

    public async Task<Result> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            if (await _userRepository.ExistsByEmailAsync(connection, command.Email, cancellationToken, transaction))
            {
                return Result.Failure(UserErrors.EmailNotUnique);
            }

            string passwordHash = _passwordHasher.Hash(command.Password);

            var user = User.Create(command.Email, command.Username, passwordHash);

            await _userRepository.CreateAsync(connection, user, cancellationToken, transaction);

            var budget = new BudgetApiResponse(Guid.NewGuid(), user.Id, DefaultBuyingPower);

            await _budgetsApi.CreateAsync(connection, budget, transaction, cancellationToken);

            var userRole = new UserRoleApiResponse(user.Id, Role.MemberId);

            await _rolesApi.CreateUserRoleAsync(connection, userRole, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
