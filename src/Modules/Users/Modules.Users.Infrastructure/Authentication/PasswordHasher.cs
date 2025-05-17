using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Modules.Users.Application.Abstractions.Authentication;

namespace Modules.Users.Infrastructure.Authentication;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA512;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 500000;

    public string Hash(string password) =>
        Convert.ToBase64String(HashPasswordInternal(password));

    public bool Verify(string password, string passwordHash)
    {
        byte[] decodedHashedPassword = Convert.FromBase64String(passwordHash);
        if (decodedHashedPassword.Length == 0)
        {
            return false;
        }

        bool verified = VerifyPasswordHashInternal(decodedHashedPassword, password);

        return verified;
    }

    private static byte[] HashPasswordInternal(string password)
    {
        byte[] salt = GetRandomSalt();

        byte[] subKey = KeyDerivation.Pbkdf2(password, salt, Prf, Iterations, HashSize);

        byte[] outputBytes = new byte[salt.Length + subKey.Length];

        Buffer.BlockCopy(salt, 0, outputBytes, 0, salt.Length);

        Buffer.BlockCopy(subKey, 0, outputBytes, salt.Length, subKey.Length);

        return outputBytes;
    }

    private static byte[] GetRandomSalt()
    {
        byte[] salt = new byte[SaltSize];

        RandomNumberGenerator.Fill(salt);

        return salt;
    }

    private static bool VerifyPasswordHashInternal(byte[] hashedPassword, string password)
    {
        try
        {
            byte[] salt = new byte[SaltSize];

            Buffer.BlockCopy(hashedPassword, 0, salt, 0, salt.Length);

            int subKeyLength = hashedPassword.Length - salt.Length;

            if (subKeyLength < SaltSize)
            {
                return false;
            }

            byte[] expectedSubKey = new byte[subKeyLength];

            Buffer.BlockCopy(hashedPassword, salt.Length, expectedSubKey, 0, expectedSubKey.Length);

            byte[] actualSubKey = KeyDerivation.Pbkdf2(password, salt, Prf, Iterations, subKeyLength);

            return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
        }
        catch
        {
            return false;
        }
    }
}
