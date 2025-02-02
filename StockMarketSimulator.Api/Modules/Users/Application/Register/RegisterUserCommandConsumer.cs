using MassTransit;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Events;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

internal sealed class RegisterUserCommandConsumer : IIntegrationEventHandler<RegisterUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandConsumer(
        IUserRepository userRepository,
        IDbConnectionFactory dbConnectionFactory,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _dbConnectionFactory = dbConnectionFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task Consume(ConsumeContext<RegisterUserCommand> context)
    {
        RegisterUserCommand command = context.Message;

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(context.CancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(context.CancellationToken);

        try
        {
            if (await _userRepository.ExistsByEmailAsync(connection, command.Email, context.CancellationToken, transaction))
            {
                var failureResult = Result.Failure<RegisterUserResponse>(UserErrors.EmailNotUnique);
                await context.RespondAsync(failureResult);
                return;
            }

            string passwordHash = _passwordHasher.Hash(command.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                PasswordHash = passwordHash,
            };

            await _userRepository.CreateAsync(connection, user, context.CancellationToken, transaction);

            var response = new RegisterUserResponse(user.Id);

            await transaction.CommitAsync(context.CancellationToken);

            await context.RespondAsync(Result.Success(response));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(context.CancellationToken);
            throw;
        }
    }
}
