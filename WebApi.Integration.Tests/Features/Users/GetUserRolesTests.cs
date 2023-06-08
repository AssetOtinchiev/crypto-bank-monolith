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

namespace WebApi.Integration.Tests.Features.Users;

public class GetUserRolesTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private UsersOptions _usersOptions = new();

    public GetUserRolesTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_get_user_roles()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUser();

        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync();

        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", new CancellationToken());
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");

        // Act
        var response = await client.GetFromJsonAsync<RoleModel[]>($"/users/roles?userId={createdUser.Id}");

        // Assert
        response.Should().NotBeNull();
        response.Should().ContainEquivalentOf(new RoleModel()
        {
            Name = RoleType.Administrator,
            UserId = createdUser.Id
        });
    }

    private User CreateUser()
    {
        var existingUser = new User
        {
            Email = _usersOptions.AdministratorEmail,
            Password = "123",
            RegisteredAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
            Roles = new List<Role>
            {
                new()
                {
                    Name = RoleType.Administrator,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return existingUser;
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

public class GetUserRolesValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private GetUserRoles.RequestValidator _validator;
    private UsersOptions _usersOptions = new();

    public GetUserRolesValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var createdUser = CreateAdminUser();
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync();

        var result = await _validator.TestValidateAsync(
            new GetUserRoles.Request(createdUser.Id));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory, MemberData(nameof(Guids))]
    public async Task Should_validate_empty_user_request(Guid userId)
    {
        var result = await _validator.TestValidateAsync(
            new GetUserRoles.Request(userId));
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode("users_validation_not_exist");
    }

    private User CreateAdminUser()
    {
        var existingUser = new User
        {
            Email = _usersOptions.AdministratorEmail,
            Password = "123",
            RegisteredAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
            Roles = new List<Role>
            {
                new()
                {
                    Name = RoleType.Administrator,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return existingUser;
    }

    public static IEnumerable<object[]> Guids
    {
        get
        {
            yield return new object[] {Guid.Parse("b3548ecf-8f31-4b4a-a120-1536fea7b3a7")};
            yield return new object[] {Guid.Parse("7dd27c42-87fb-4d9c-a06f-38cd0fc7de0a")};
            yield return new object[] {Guid.Empty};
        }
    }

    private User CreateUser(string email)
    {
        var existingUser = new User
        {
            Email = email,
            Password = "123",
            RegisteredAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
            Roles = new List<Role>
            {
                new()
                {
                    Name = RoleType.User,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return existingUser;
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new GetUserRoles.RequestValidator(_db);
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