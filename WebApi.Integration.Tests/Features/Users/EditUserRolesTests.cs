using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Options;
using WebApi.Features.Users.Requests;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Users;

public class EditUserRolesTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private UsersOptions _usersOptions = new();
        private CancellationToken _cancellationToken;

    public EditUserRolesTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_edit_user_roles()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser(_usersOptions.AdministratorEmail, RoleType.Administrator);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", _cancellationToken);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");

        var request = new EditUserRoles.Request(new[]
        {
            RoleType.User,
            RoleType.Administrator
        }, createdUser.Id);

        // Act
        (await client.PutAsJsonAsync("/users/roles", request, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        // Assert
        var roles = await _db.Roles.Where(x => x.UserId == createdUser.Id)
            .ToArrayAsync(cancellationToken: _cancellationToken);

        roles.Should().NotBeEmpty();
        roles.Length.Should().Be(2);
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}

public class EditUserRolesValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private EditUserRoles.RequestValidator _validator;
    private UsersOptions _usersOptions = new();
    private CancellationToken _cancellationToken;

    public EditUserRolesValidatorTests(TestingWebAppFactory<Program> factory)
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
            new EditUserRoles.Request(new[]
            {
                RoleType.User,
                RoleType.Administrator
            }, createdUser.Id), cancellationToken: _cancellationToken);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_require_roles()
    {
        var createdUser = CreateUserMock.CreateUser(_usersOptions.AdministratorEmail, RoleType.Administrator);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(
            new EditUserRoles.Request(Array.Empty<RoleType>(), createdUser.Id), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Roles)
            .WithErrorCode("users_validation_role_required");
    }

    [Fact]
    public async Task Should_require_user_id()
    {
        var userId = Guid.NewGuid();
        var result = await _validator.TestValidateAsync(
            new EditUserRoles.Request(new[]
            {
                RoleType.User,
                RoleType.Administrator
            }, userId), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode("general_validation_user_not_exist");
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new EditUserRoles.RequestValidator(_db);
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}