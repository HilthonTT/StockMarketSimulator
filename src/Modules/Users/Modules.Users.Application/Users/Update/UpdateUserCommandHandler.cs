using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Application.Users.Update;

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId != request.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        Result<Username> usernameResult = Username.Create(request.Username);
        if (usernameResult.IsFailure)
        {
            return usernameResult;
        }

        Username username = usernameResult.Value;

        Option<User> optionUser = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        User user = optionUser.ValueOrThrow();

        user.ChangeUsername(username);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
