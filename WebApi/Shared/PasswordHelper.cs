using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using WebApi.Features.Auth.Options;

namespace WebApi.Shared;

public class PasswordHelper
{
    public static byte[] GetSecureSalt()
    {
        return RandomNumberGenerator.GetBytes(32);
    }
    public static string HashUsingArgon2(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = salt;
        argon2.DegreeOfParallelism = ArgonSecurityOptions.Argon2IdParameters.DegreeOfParallelism; // four cores
        argon2.Iterations = ArgonSecurityOptions.Argon2IdParameters.Iterations;
        argon2.MemorySize = ArgonSecurityOptions.Argon2IdParameters.MemorySize; // 1 GB

        var bytes = argon2.GetBytes(16);
        
        return Convert.ToBase64String(bytes);
    }
    public static string GetHexUsingArgon2T(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = salt;
        argon2.DegreeOfParallelism = ArgonSecurityOptions.Argon2IdParameters.DegreeOfParallelism; // four cores
        argon2.Iterations = ArgonSecurityOptions.Argon2IdParameters.Iterations;
        argon2.MemorySize = ArgonSecurityOptions.Argon2IdParameters.MemorySize; // 1 GB

        var bytes = argon2.GetBytes(16);
        var hash = Convert.ToBase64String(bytes);
        
        return 
            $"$argon2i$m={argon2.MemorySize},t={argon2.Iterations},p={argon2.DegreeOfParallelism}${Convert.ToBase64String(argon2.Salt)}${hash}";
    }


    public static (string salt, string hash) GetHashFromHexArgon2(string hex)
    {
        var splittedHex = hex.Split("$");
        var salt = splittedHex[3];
        var hash = splittedHex[4];

        return new(salt, hash);
    }
}