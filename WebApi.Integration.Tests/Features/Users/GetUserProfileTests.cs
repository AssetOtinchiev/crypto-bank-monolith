using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Models;
using WebApi.Features.Users.Options;
using WebApi.Features.Users.Requests;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Users;

public class GetUserProfileTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private UsersOptions _usersOptions = new();
    private CancellationToken _cancellationToken;

    public GetUserProfileTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_get_user_profile()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser(_usersOptions.AdministratorEmail, RoleType.Administrator);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", _cancellationToken);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");

        // Act
        var response = await client.GetFromJsonAsync<UserModel>($"/users", cancellationToken: _cancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Email.Should().MatchEquivalentOf(createdUser.Email);
        response.DateOfBirth.Should().Be(new DateTime(2000, 01, 31).ToUniversalTime());
    }

    [Fact]
    public async Task Should_validate_auth_token()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization",
            $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiJhYmNkMTIzIiwiZXhwaXJ5IjoxNjQ2NjM1NjExMzAxfQ.");

        // Act
        var response = await client.GetAsync($"/users", cancellationToken: _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync()
    {
        BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        BaseInitializeHelper.DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}

public class GetUserProfileValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private GetUserProfile.RequestValidator _validator;
    private UsersOptions _usersOptions = new();
    private CancellationToken _cancellationToken;

    public GetUserProfileValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var createdUser = CreateUserMock.CreateUser(_usersOptions.AdministratorEmail, RoleType.Administrator);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(
            new GetUserProfile.Request(createdUser.Id), cancellationToken: _cancellationToken);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory, MemberData(nameof(RandomGuidMock.Guids), MemberType = typeof(RandomGuidMock))]
    public async Task Should_validate_empty_user_request(Guid userId)
    {
        var result = await _validator.TestValidateAsync(
            new GetUserProfile.Request(userId), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode("users_validation_not_exist");
    }

    public Task InitializeAsync()
    {
        BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
        _validator = new GetUserProfile.RequestValidator(_db);
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        BaseInitializeHelper.DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}