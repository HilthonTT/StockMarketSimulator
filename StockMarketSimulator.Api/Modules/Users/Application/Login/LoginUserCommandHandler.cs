﻿using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Application.Login;

internal sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, TokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<LoginUserCommand> _validator;
    private readonly ITokenProvider _tokenProvider;

    public LoginUserCommandHandler(
        IUserRepository userRepository, 
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IDbConnectionFactory dbConnectionFactory,
        IValidator<LoginUserCommand> validator,
        ITokenProvider tokenProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<TokenResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<TokenResponse>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        Result<Email> emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<TokenResponse>(emailResult.Error);
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        User? user = await _userRepository.GetByEmailAsync(connection, command.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        bool verified = _passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        string token = await _tokenProvider.Create(connection, user);

        var refreshToken = RefreshToken.Create(
            _tokenProvider.GenerateRefreshToken(),
            user.Id);

        await _refreshTokenRepository.CreateAsync(connection, refreshToken, cancellationToken: cancellationToken);

        return new TokenResponse(token, refreshToken.Token);
    }
}
