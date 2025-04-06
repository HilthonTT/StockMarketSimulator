using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginUserCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<TokenResponse>(emailResult.Error);
        }

        Option<User> userOption = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (!userOption.IsSome)
        {
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        User user = userOption.ValueOrThrow();
        if (!user.EmailVerified)
        {
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        bool verified = passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!verified)
        {
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        string token = tokenProvider.Create(user);

        var refreshToken = RefreshToken.Create(tokenProvider.GenerateRefreshToken(), user.Id);

        refreshTokenRepository.Insert(refreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(token, refreshToken.Token);
    }
}
