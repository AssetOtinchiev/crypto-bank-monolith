using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Requests;
using WebApi.Shared;

namespace WebApi.Integration.Tests.Features.Users;

public class RegisterUserTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;

    public RegisterUserTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_register_user()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        (await client.PostAsJsonAsync("/users", new
            {
                Email = "test@test.com",
                Password = "qwerty123456A!",
                DateOfBirth = "2000-01-31",
            }))
            .EnsureSuccessStatusCode();

        // Assert
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == "test@test.com");
        user.Should().NotBeNull();
        user!.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        user.DateOfBirth.Date.Should().Be(new DateTime(2000, 01, 31).Date);

        var passwordHelper = _scope.ServiceProvider.GetRequiredService<PasswordHelper>();
        passwordHelper.VerifyPassword("qwerty123456A!", user.Password).Should()
            .BeTrue();
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}

public class RegisterValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private RegisterUser.RequestValidator _validator;

    public RegisterValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_email(string email)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode("users_validation_email_required");
    }

    [Theory]
    [InlineData("test")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public async Task Should_validate_email_format(string email)
    {
        var result = await _validator.TestValidateAsync(new
            RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode("users_validation_email_invalid_format");
    }

    [Fact]
    public async Task Should_validate_email_taken()
    {
        const string email = "test@test.com";

        var existingUser = new User
        {
            Email = email,
            Password = "123",
            RegisteredAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
        };

        _db.Users.Add(existingUser);
        await _db.SaveChangesAsync();

        var result = await _validator.TestValidateAsync(new
            RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode("users_validation_email_exist_or_invalid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_password(string password)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("users_validation_password_required");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("123456")]
    public async Task Should_validate_password_length(string password)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("users_validation_password_short");
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new RegisterUser.RequestValidator(_db);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}