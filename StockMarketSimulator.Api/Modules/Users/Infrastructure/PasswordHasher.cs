using System.Security.Cryptography;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 64;
    private const int Iterations = 600_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToBase64String(hash)}:{Convert.ToBase64String(salt)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split(':');

        if (parts.Length < 2) 
        { 
            return false;
        }

        byte[] hash = Convert.FromBase64String(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}
