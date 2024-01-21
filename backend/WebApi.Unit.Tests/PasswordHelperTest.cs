using FluentAssertions;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using WebApi.Features.Auth.Options;
using WebApi.Shared;

namespace WebApi.Unit.Tests;

public class PasswordHelperTest
{
    [Fact]
    public async Task HashPassword_should_embed_parameters_salt_and_hash_into_result()
    {
        // Arrange
        var options = new ArgonSecurityOptions
        {
            DegreeOfParallelism = 4,
            MemorySize = 4096,
            Iterations = 3,
        };

        var passwordHelper = new PasswordHelper(Options.Create(options));

        // Act
        var passwordHash = passwordHelper.GetHashUsingArgon2("test_password");
        var splittedHex = passwordHash.Split("$");

        var salt = splittedHex[3];
        var hash = splittedHex[4];
        var argon2 = splittedHex[1];

        var payload = splittedHex[2];
        var settings = payload.Split(",");
        var memorySize = int.Parse(settings[0].Substring(2));
        var iterations = int.Parse(settings[1].Substring(2));
        var degreeOfParallelism = int.Parse(settings[2].Substring(2));

        memorySize.Should().Be(options.MemorySize);
        iterations.Should().Be(options.Iterations);
        degreeOfParallelism.Should().Be(options.DegreeOfParallelism);
        argon2.Should().Be("argon2id");

        var saltBytes = Convert.FromBase64String(salt);
        using var argon2Id = new Argon2id("test_password"u8.ToArray())
        {
            Salt = saltBytes,
            DegreeOfParallelism = options.DegreeOfParallelism,
            MemorySize = options.MemorySize,
            Iterations = options.Iterations,
        };

        var hashBytes = Convert.FromBase64String(hash);
        var expectedHashBytes = argon2Id.GetBytes(16);
        hashBytes.Should().BeEquivalentTo(expectedHashBytes);
    }

    [Fact]
    public void VerifyHashedPassword_should_return_true_for_correct_hash()
    {
        // Arrange
        var hash = "$argon2id$m=4096,t=3,p=4$QIcEkS4Smvv8/40k918ZVvF7xoaiLbJQ8y53iOTt/eM=$5QMWJc99U/Y2LKMmsRtOAg==";

        var options = new ArgonSecurityOptions
        {
            DegreeOfParallelism = 4,
            MemorySize = 4096,
            Iterations = 3,
        };

        var hasher = new PasswordHelper(Options.Create(options));

        // Act
        var result = hasher.VerifyPassword("test_password", hash);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    // Changing each parameter should result in a different hash
    [InlineData("$argon2id$m=4098,t=3,p=4$QIcEkS4Smvv8/40k918ZVvF7xoaiLbJQ8y53iOTt/eM=$5QMWJc99U/Y2LKMmsRtOAg==")]
    [InlineData("$argon2id$m=4096,t=4,p=4$QIcEkS4Smvv8/40k918ZVvF7xoaiLbJQ8y53iOTt/eM=$5QMWJc99U/Y2LKMmsRtOAg==")]
    [InlineData("$argon2id$m=4096,t=3,p=5$QIcEkS4Smvv8/40k918ZVvF7xoaiLbJQ8y53iOTt/eM=$5QMWJc99U/Y2LKMmsRtOAg==")]
    [InlineData("$argon2id$m=4096,t=3,p=4$QIcEkS4Smvv8/40k918ZVvF7xoaiLbJQ8y53iOTt/eM=$6QMWJc99U/Y2LKMmsRtOAg==")]
    public void VerifyHashedPassword_should_return_false_if_parameters_or_salt_or_hash_modified(string hash)
    {
        // Arrange
        var options = new ArgonSecurityOptions
        {
            DegreeOfParallelism = 4,
            MemorySize = 4096,
            Iterations = 3,
        };

        var hasher = new PasswordHelper(Options.Create(options));

        // Act
        var result = hasher.VerifyPassword("test_password", hash);

        // Assert
        result.Should().BeFalse();
    }
}