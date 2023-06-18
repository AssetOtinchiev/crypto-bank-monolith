using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Requests;
using WebApi.Features.Users.Domain;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;
using WebApi.Shared;

namespace WebApi.Integration.Tests.Features.Auth;

public class AuthenticateTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    public AuthenticateTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_authenticate_user()
    {
        // Arrange
        var client = _factory.CreateClient();
        var password = "qwerty123456A!";
        var email = "test@test.com";
        var passwordHelper = _scope.ServiceProvider.GetRequiredService<PasswordHelper>();
        var hashPassword = passwordHelper.GetHashUsingArgon2(password);
        var createdUser = CreateUserMock.CreateUser(email, RoleType.User, hashPassword);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        // Act
        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = email,
            Password = password
        }, cancellationToken: _cancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        var accessTokenModel =
            await response.Content.ReadFromJsonAsync<AccessTokenModel>(cancellationToken: _cancellationToken);
        accessTokenModel.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_require_email()
    {
        // Arrange
        var client = _factory.CreateClient();
        (await client.PostAsJsonAsync("/users", new
            {
                Email = "test@test.com",
                Password = "qwerty123456A!",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
            }, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        // Act
        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = "invalidMail@test.com",
            Password = "qwerty123456A!"
        }, cancellationToken: _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_require_password()
    {
        // Arrange
        var client = _factory.CreateClient();
        (await client.PostAsJsonAsync("/users", new
            {
                Email = "test@test.com",
                Password = "qwerty123456A!",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
            }, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        // Act
        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = "test@test.com",
            Password = "easyPassword"
        }, cancellationToken: _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public Task InitializeAsync()
    {
        new BaseInitializeHelper().Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        new BaseInitializeHelper().DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}

public class AuthenticateValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private Authenticate.RequestValidator _validator;
    private CancellationToken _cancellationToken;

    public AuthenticateValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("adsadas", "asdasdAAA")]
    [InlineData("email", "PaaSSWORD")]
    [InlineData("", "PaaSSWORD")]
    public async Task Should_require_email(string email, string password)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, password),
            cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorCode("auth_validation_invalid_credential");
    }

    [Theory]
    [InlineData("adsadas@gmail.com", "aa")]
    [InlineData("email@gmail.com", " ")]
    public async Task Should_require_password(string email, string password)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, password),
            cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth_validation_invalid_credential");
    }

    public Task InitializeAsync()
    {
        new BaseInitializeHelper().Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
        _validator = new Authenticate.RequestValidator();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        new BaseInitializeHelper().DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}