using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Authentication.LoginWithRefreshToken;

internal sealed class LoginUserWithRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginUserWithRefreshTokenCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginUserWithRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Option<RefreshToken> optionRefreshToken = 
            await refreshTokenRepository.GetByTokenHashAsync(
                tokenProvider.HashRefreshToken(request.RefreshToken),
                cancellationToken);

        if (!optionRefreshToken.IsSome)
        {
            return Result.Failure<TokenResponse>(RefreshTokenErrors.Expired);
        }

        RefreshToken refreshToken = optionRefreshToken.ValueOrThrow();
        if (refreshToken.IsExpired())
        {
            return Result.Failure<TokenResponse>(RefreshTokenErrors.Expired);
        }

        string accessToken = tokenProvider.Create(refreshToken.User);

        string newPlainRefreshToken = tokenProvider.GenerateRefreshToken();

        refreshToken.Refresh(tokenProvider.HashRefreshToken(newPlainRefreshToken));

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request rotated (or revoked) this token first: the presented token is no longer valid.
            return Result.Failure<TokenResponse>(RefreshTokenErrors.Expired);
        }

        return new TokenResponse(accessToken, newPlainRefreshToken);
    }
}
