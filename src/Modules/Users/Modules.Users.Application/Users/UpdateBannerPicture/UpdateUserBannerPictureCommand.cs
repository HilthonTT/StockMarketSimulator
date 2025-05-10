using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Modules.Users.Application.Users.UpdateBannerPicture;

public sealed record UpdateUserBannerPictureCommand(Guid UserId, IFormFile File) : ICommand;
