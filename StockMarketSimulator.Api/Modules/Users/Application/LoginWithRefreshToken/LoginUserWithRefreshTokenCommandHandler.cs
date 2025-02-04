using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.LoginWithRefreshToken;

internal sealed class LoginUserWithRefreshTokenCommandHandler : ICommandHandler<LoginUserWithRefreshTokenCommand, TokenResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IValidator<LoginUserWithRefreshTokenCommand> _validator;

    public LoginUserWithRefreshTokenCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenProvider tokenProvider,
        IValidator<LoginUserWithRefreshTokenCommand> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenProvider = tokenProvider;
        _validator = validator;
    }

    public async Task<Result<TokenResponse>> Handle(
        LoginUserWithRefreshTokenCommand command, 
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<TokenResponse>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        RefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            connection, 
            command.RefreshToken, 
            cancellationToken: cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            return Result.Failure<TokenResponse>(RefreshTokenErrors.Expired);
        }

        User? user = await _userRepository.GetByIdAsync(connection, refreshToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<TokenResponse>(RefreshTokenErrors.Expired);
        }

        string accessToken = _tokenProvider.Create(user);

        refreshToken.Token = _tokenProvider.GenerateRefreshToken();
        refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(7);

        await _refreshTokenRepository.UpdateAsync(connection, refreshToken, cancellationToken: cancellationToken);

        return new TokenResponse(accessToken, refreshToken.Token);
    }
}
