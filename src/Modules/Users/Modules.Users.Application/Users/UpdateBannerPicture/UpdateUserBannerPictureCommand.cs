using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Users.UpdateBannerPicture;

/// <summary>
/// Replaces the user's banner image. The caller owns <paramref name="Content"/> and disposes it after
/// the command completes; the Application layer never sees ASP.NET types such as <c>IFormFile</c>.
/// </summary>
public sealed record UpdateUserBannerPictureCommand(Guid UserId, Stream Content, string FileName, string ContentType) : ICommand;
