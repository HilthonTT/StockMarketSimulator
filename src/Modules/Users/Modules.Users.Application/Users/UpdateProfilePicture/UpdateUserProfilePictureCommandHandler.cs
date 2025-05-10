using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Storage;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Users.UpdateProfilePicture;

internal sealed class UpdateUserProfilePictureCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository,
    IBlobService blobService,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserProfilePictureCommand>
{
    public async Task<Result> Handle(UpdateUserProfilePictureCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        Option<User> optionUser = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        User user = optionUser.ValueOrThrow();

        Guid? previousImageId = user.ProfileImageId;

        using Stream stream = request.File.OpenReadStream();

        Guid newImageId = await blobService.UploadAsync(stream, request.File.ContentType, cancellationToken);

        user.ChangeProfileImage(newImageId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousImageId.HasValue)
        {
            await blobService.DeleteAsync(previousImageId.Value, cancellationToken);
        }

        return Result.Success();
    }
}
