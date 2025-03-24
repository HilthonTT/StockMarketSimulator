using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokensCommandHandler : ICommandHandler<RevokeRefreshTokensCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserContext _userContext;
    private readonly IValidator<RevokeRefreshTokensCommand> _validator;

    public RevokeRefreshTokensCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IRefreshTokenRepository refreshTokenRepository,
        IUserContext userContext,
        IValidator<RevokeRefreshTokensCommand> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _refreshTokenRepository = refreshTokenRepository;
        _userContext = userContext;
        _validator = validator;
    }

    public async Task<Result> Handle(
        RevokeRefreshTokensCommand command, 
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        if (_userContext.UserId != command.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        await _refreshTokenRepository.DeleteByUserIdAsync(connection, _userContext.UserId, cancellationToken: cancellationToken);

        return Result.Success();
    }
}
