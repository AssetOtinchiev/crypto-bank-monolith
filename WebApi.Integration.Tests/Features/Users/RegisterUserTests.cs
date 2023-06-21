using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Requests;
using WebApi.Integration.Tests.Helpers;
using WebApi.Shared;

namespace WebApi.Integration.Tests.Features.Users;

public class RegisterUserTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    public RegisterUserTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_register_user()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dateBirth = DateTime.UtcNow.AddYears(-20);
        // Act
        (await client.PostAsJsonAsync("/users", new
            {
                Email = "test@test.com",
                Password = "qwerty123456A!",
                DateOfBirth = dateBirth,
            }, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        // Assert
        var user = await _db.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Email == "test@test.com",
                cancellationToken: _cancellationToken);
        user.Should().NotBeNull();
        user!.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        user.DateOfBirth.Date.Should().Be(dateBirth.Date);
        user.Roles.First().Name.Should().Be(RoleType.User);

        var passwordHelper = _scope.ServiceProvider.GetRequiredService<PasswordHelper>();
        passwordHelper.VerifyPassword("qwerty123456A!", user.Password).Should()
            .BeTrue();
    }

    [Fact]
    public async Task Should_validate_duplicate_user()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "test@test.com";
        _db.Users.Add(new User()
        {
            Email = email,
            Password = "123123",
            RegisteredAt = DateTime.UtcNow,
            DateOfBirth = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(_cancellationToken);

        var dateBirth = DateTime.UtcNow.AddYears(-20);
        // Act
        var result = await client.PostAsJsonAsync("/users", new
        {
            Email = email,
            Password = "qwerty123456A!",
            DateOfBirth = dateBirth,
        }, cancellationToken: _cancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public Task InitializeAsync()
    {
        BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        BaseInitializeHelper.DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}

public class RegisterValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private RegisterUser.RequestValidator _validator;
    private CancellationToken _cancellationToken;

    public RegisterValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", "password", new DateTime(2000, 01, 31).ToUniversalTime()),
            cancellationToken: _cancellationToken);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_email(string email)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()),
            cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode("users_validation_email_required");
    }

    [Theory]
    [InlineData("test")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public async Task Should_validate_email_format(string email)
    {
        var result = await _validator.TestValidateAsync(new
                RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()),
            cancellationToken: _cancellationToken);
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
        await _db.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(new
                RegisterUser.Request(email, "password", new DateTime(2000, 01, 31).ToUniversalTime()),
            cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode("users_validation_email_exist_or_invalid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_password(string password)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()),
            cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("users_validation_password_required");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("123456")]
    public async Task Should_validate_password_length(string password)
    {
        var result = await _validator.TestValidateAsync(
            new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31).ToUniversalTime()),
            cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("users_validation_password_short");
    }

    public Task InitializeAsync()
    {
        BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
        _validator = new RegisterUser.RequestValidator(_db);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        BaseInitializeHelper.DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}