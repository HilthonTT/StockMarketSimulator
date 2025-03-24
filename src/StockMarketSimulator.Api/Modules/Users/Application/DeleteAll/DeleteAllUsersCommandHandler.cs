using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Application.DeleteAll;

internal sealed class DeleteAllUsersCommandHandler : ICommandHandler<DeleteAllUsersCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserRepository _userRepository;

    public DeleteAllUsersCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserRepository userRepository)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteAllUsersCommand command, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        await _userRepository.DeleteAllAsync(connection, cancellationToken: cancellationToken);

        return Result.Success();
    }
}
