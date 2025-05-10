using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Modules.Users.Application.Users.UpdateProfilePicture;

public sealed record UpdateUserProfilePictureCommand(Guid UserId, IFormFile File) : ICommand;
