using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Users.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokensCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserContext userContext) : ICommandHandler<RevokeRefreshTokensCommand>
{
    public async Task<Result> Handle(RevokeRefreshTokensCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId != request.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        await refreshTokenRepository.BatchDeleteAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
