using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using WebApi.Features.Auth.Options;
using WebApi.Shared.Models;

namespace WebApi.Shared;

public class PasswordHelper
{
    private readonly ArgonSecurityOptions _argonSecurityOptions;

    public PasswordHelper(IOptions<ArgonSecurityOptions> argonSecurityOptions)
    {
        _argonSecurityOptions = argonSecurityOptions.Value;
    }

    private byte[] GetSecureSalt()
    {
        return RandomNumberGenerator.GetBytes(32);
    }

    private string HashUsingArgon2WithDbParam(string password, byte[] salt, int degreeOfParallelism, int iterations, int memorySize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = salt;
        argon2.DegreeOfParallelism = degreeOfParallelism;
        argon2.Iterations = iterations;
        argon2.MemorySize = memorySize;

        var bytes = argon2.GetBytes(16);

        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var passwordHex = GetSettingsFromHexArgon2(passwordHash);
        
        var hash = HashUsingArgon2WithDbParam(password, Convert.FromBase64String(passwordHex.Salt),
            passwordHex.DegreeOfParallelism, passwordHex.Iterations, passwordHex.MemorySize);

        return passwordHex.Hash == hash;
    }

    public string GetHashUsingArgon2(string password)
    {
        var salt = GetSecureSalt();
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = salt;
        argon2.DegreeOfParallelism = _argonSecurityOptions.DegreeOfParallelism; // four cores
        argon2.Iterations = _argonSecurityOptions.Iterations;
        argon2.MemorySize = _argonSecurityOptions.MemorySize; // 1 GB

        var bytes = argon2.GetBytes(16);
        var hash = Convert.ToBase64String(bytes);

        return
            $"$argon2id$m={argon2.MemorySize},t={argon2.Iterations},p={argon2.DegreeOfParallelism}${Convert.ToBase64String(argon2.Salt)}${hash}";
    }
    
    private SettingsFromHexArgon GetSettingsFromHexArgon2(string hex)
    {
        var splittedHex = hex.Split("$");

        var salt = splittedHex[3];
        var hash = splittedHex[4];
        
        var payload = splittedHex[2];
        var settings = payload.Split(",");
        var memorySize = int.Parse(settings[0].Substring(2));
        var iterations = int.Parse(settings[1].Substring(2));
        var degreeOfParallelism = int.Parse(settings[2].Substring(2));

        return new SettingsFromHexArgon
        {
            Salt = salt,
            Hash = hash,
            Iterations = iterations,
            MemorySize = memorySize,
            DegreeOfParallelism = degreeOfParallelism
        };;
    }
}