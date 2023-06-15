using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Requests;
using WebApi.Integration.Tests.Helpers;

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
            Password = "qwerty123456A!"
        }, cancellationToken: _cancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        var accessTokenModel = await response.Content.ReadFromJsonAsync<AccessTokenModel>(cancellationToken: _cancellationToken);
        accessTokenModel.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Users.RemoveRange(_db.Users);
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
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, password));
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorCode("auth_validation_invalid_credential");
    }

    [Theory]
    [InlineData("adsadas@gmail.com", "aa")]
    [InlineData("email@.com", " ")]
    public async Task Should_require_password(string email, string password)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, password));
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorCode("auth_validation_invalid_credential");
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new Authenticate.RequestValidator();
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Roles.RemoveRange(_db.Roles);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}