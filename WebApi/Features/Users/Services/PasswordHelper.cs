using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace WebApi.Features.Users.Services;

public class PasswordHelper
{
    public static byte[] GetSecureSalt()
    {
        return RandomNumberGenerator.GetBytes(32);
    }
    public static string HashUsingPbkdf2(string password, byte[] salt)
    {
        byte[] derivedKey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iterationCount: 100000, 32);

        return Convert.ToBase64String(derivedKey);
    }
}