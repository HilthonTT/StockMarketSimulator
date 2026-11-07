using Modules.Users.Domain.Entities;

namespace Modules.Users.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);

    string GenerateRefreshToken();

    /// <summary>
    /// Hashes a refresh token for storage and lookup (SHA-256, lower-case hex). Only hashes are persisted.
    /// </summary>
    string HashRefreshToken(string refreshToken);
}
