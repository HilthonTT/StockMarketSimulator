using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        Result<Password> passwordResult = Password.Create(request.NewPassword);
        if (passwordResult.IsFailure)
        {
            return passwordResult;
        }

        Option<User> optionUser = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        User user = optionUser.ValueOrThrow();

        bool verified = passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!verified)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        string newPasswordHash = passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(newPasswordHash);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // A password change must end every existing session, otherwise a stolen refresh token
        // keeps working after the victim changes their password.
        await refreshTokenRepository.BatchDeleteAsync(user.Id, cancellationToken);

        return Result.Success();
    }
}
