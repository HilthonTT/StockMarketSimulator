using FluentValidation;
using Npgsql;
using Quartz;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;
using StockMarketSimulator.Api.Modules.Users.Infrastructure.Jobs;

namespace StockMarketSimulator.Api.Modules.Users.Application.ChangePassword;

internal sealed class ChangeUserPasswordCommandHandler : ICommandHandler<ChangeUserPasswordCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<ChangeUserPasswordCommand> _validator;
    private readonly ISchedulerFactory _schedulerFactory;

    public ChangeUserPasswordCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserContext userContext,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IValidator<ChangeUserPasswordCommand> validator,
        ISchedulerFactory schedulerFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userContext = userContext;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _schedulerFactory = schedulerFactory;
    }

    public async Task<Result> Handle(ChangeUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        Result validationResult = await ValidateCommand(command, cancellationToken);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        User? user = await _userRepository.GetByIdAsync(connection, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (!_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        await UpdatePassword(connection, user, command.NewPassword, cancellationToken);

        await SchedulePasswordChangeNotification(user, cancellationToken);

        return Result.Success();
    }

    private async Task<Result> ValidateCommand(ChangeUserPasswordCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        return command.UserId == _userContext.UserId
            ? Result.Success()
            : Result.Failure(UserErrors.Unauthorized);
    }

    private Task UpdatePassword(NpgsqlConnection connection, User user, string newPassword, CancellationToken cancellationToken)
    {
        string newPasswordHash = _passwordHasher.Hash(newPassword);

        user.ChangePassword(newPasswordHash);

        return _userRepository.UpdateAsync(connection, user, cancellationToken);
    }

    private async Task SchedulePasswordChangeNotification(User user, CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobData = new JobDataMap
        {
            { "email", user.Email },
            { "username", user.Username }
        };

        IJobDetail job = JobBuilder.Create<PasswordChangedNotifierJob>()
            .WithIdentity($"notifier-{Guid.NewGuid()}", "notifiers")
            .SetJobData(jobData)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{Guid.NewGuid()}", "notifiers")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}
