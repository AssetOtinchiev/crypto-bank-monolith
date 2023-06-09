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

namespace WebApi.Integration.Tests.Features.Users;

public class GetUserProfileTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private UsersOptions _usersOptions = new();

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
        await _db.SaveChangesAsync();

        // Act
        var response = await client.GetFromJsonAsync<UserModel>($"/users");

        // Assert
        response.Should().NotBeNull();
        response.Email.Should().MatchEquivalentOf(createdUser.Email);
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
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

    public GetUserProfileValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var createdUser = CreateUserMock.CreateUser(_usersOptions.AdministratorEmail, RoleType.Administrator);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync();

        var result = await _validator.TestValidateAsync(
            new GetUserProfile.Request(createdUser.Id));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory, MemberData(nameof(RandomGuidMock.Guids), MemberType = typeof(RandomGuidMock))]
    public async Task Should_validate_empty_user_request(Guid userId)
    {
        var result = await _validator.TestValidateAsync(
            new GetUserProfile.Request(userId));
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode("users_validation_not_exist");
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new GetUserProfile.RequestValidator(_db);
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}