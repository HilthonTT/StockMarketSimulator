using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Users.LoginWithRefreshToken;

internal sealed class LoginUserWithRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginUserWithRefreshTokenCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginUserWithRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Option<RefreshToken> optionRefreshToken = 
            await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

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

        refreshToken.Refresh(tokenProvider.GenerateRefreshToken());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(accessToken, refreshToken.Token);
    }
}
