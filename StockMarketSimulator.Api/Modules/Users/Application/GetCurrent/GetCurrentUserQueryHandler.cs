using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.GetCurrent;

internal sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetCurrentUserQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserRepository userRepository,
        IUserContext userContext)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<UserResponse>> Handle(
        GetCurrentUserQuery query, 
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        User? user = await _userRepository.GetByIdAsync(
            connection,
            _userContext.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.Unauthorized);
        }

        return new UserResponse(user.Id, user.Email, user.Username);
    }
}
