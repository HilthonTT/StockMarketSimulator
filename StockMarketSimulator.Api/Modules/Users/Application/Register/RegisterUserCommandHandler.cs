using FluentValidation;
using Npgsql;
using Quartz;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Budgets.Api;
using StockMarketSimulator.Api.Modules.Roles.Api;
using StockMarketSimulator.Api.Modules.Roles.Domain;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;
using StockMarketSimulator.Api.Modules.Users.Infrastructure.Jobs;

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
    private readonly ISchedulerFactory _schedulerFactory;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IDbConnectionFactory dbConnectionFactory,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserCommand> validator,
        IBudgetsApi budgetsApi,
        IRolesApi rolesApi,
        ISchedulerFactory schedulerFactory)
    {
        _userRepository = userRepository;
        _dbConnectionFactory = dbConnectionFactory;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _budgetsApi = budgetsApi;
        _rolesApi = rolesApi;
        _schedulerFactory = schedulerFactory;
    }

    public async Task<Result> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        // Validate the command
        var validationResult = await ValidateCommand(command, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create user
            Result<User> userResult = await CreateUser(command, connection, transaction, cancellationToken);
            if (userResult.IsFailure)
            {
                return userResult;
            }

            User user = userResult.Value;

            // Assign user role
            await AssignUserRole(user, connection, transaction, cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);

            // Schedule email verification job
            await ScheduleEmailVerification(command.Email, cancellationToken);

            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Result> ValidateCommand(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        return validationResult.IsValid
            ? Result.Success()
            : Result.Failure(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
    }

    private async Task<Result<User>> CreateUser(
        RegisterUserCommand command,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        var passwordResult = Password.Create(command.Password);

        var validation = Result.FirstFailureOrSuccess(emailResult, passwordResult);
        if (validation.IsFailure)
        {
            return Result.Failure<User>(validation.Error);
        }

        if (await _userRepository.ExistsByEmailAsync(connection, command.Email, cancellationToken, transaction))
        {
            return Result.Failure<User>(UserErrors.EmailNotUnique);
        }

        string passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Create(command.Email, command.Username, passwordHash);

        await _userRepository.CreateAsync(connection, user, cancellationToken, transaction);

        // Initialize user's budget
        var budget = new BudgetApiResponse(Guid.NewGuid(), user.Id, DefaultBuyingPower);
        await _budgetsApi.CreateAsync(connection, budget, transaction, cancellationToken);

        return Result.Success(user);
    }

    private Task AssignUserRole(
        User user,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var userRole = new UserRoleApiResponse(user.Id, Role.MemberId);

        return _rolesApi.CreateUserRoleAsync(connection, userRole, transaction, cancellationToken);
    }

    private async Task ScheduleEmailVerification(string email, CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobData = new JobDataMap
        {
            { "email", email },
            { "verification-token", Guid.CreateVersion7() },
            { "verification-endpoint", "endpoint" }
        };

        IJobDetail job = JobBuilder.Create<EmailVerificationJob>()
            .WithIdentity($"auth-{Guid.CreateVersion7()}", "auth")
            .SetJobData(jobData)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{Guid.NewGuid()}", "auth")
            .StartAt(DateTime.UtcNow)
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}
