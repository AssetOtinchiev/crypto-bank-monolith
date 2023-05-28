using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace WebApi.Shared;

public class PasswordHelper
{
    public static byte[] GetSecureSalt()
    {
        var buffer = new byte[32];
        using var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(buffer);
        return buffer;
    }
    public static string HashUsingArgon2(string password, byte[] salt)
    {
       var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = salt;
        argon2.DegreeOfParallelism = 8; // four cores
        argon2.Iterations = 4;
        argon2.MemorySize = 1024 * 1024; // 1 GB

        var bytes = argon2.GetBytes(16);
        
        
        return Convert.ToBase64String(bytes);
    }
}