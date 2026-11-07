using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Users.UpdateProfilePicture;

/// <summary>
/// Replaces the user's profile image. The caller owns <paramref name="Content"/> and disposes it after
/// the command completes; the Application layer never sees ASP.NET types such as <c>IFormFile</c>.
/// </summary>
public sealed record UpdateUserProfilePictureCommand(Guid UserId, Stream Content, string FileName, string ContentType) : ICommand;
