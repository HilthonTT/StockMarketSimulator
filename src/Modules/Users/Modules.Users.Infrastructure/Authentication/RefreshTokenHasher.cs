using System.Security.Cryptography;
using System.Text;

namespace Modules.Users.Infrastructure.Authentication;

/// <summary>
/// Refresh tokens are 256 bits of randomness, so a fast unsalted SHA-256 is enough: there is nothing to
/// brute-force, and the hash must be deterministic so it can be looked up by an index.
/// </summary>
internal static class RefreshTokenHasher
{
    public static string Hash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(refreshToken);

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexStringLower(hash);
    }
}
