using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Accounts.Requests;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Integration.Tests.Features.Users.MockData;

namespace WebApi.Integration.Tests.Features.Accounts;

public class GetAccountsTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;

    public GetAccountsTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_get_accounts()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        var account = new Account()
        {
            Amount = 10,
            Currency = "btc",
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };
        createdUser.Accounts.Add(account);
        _db.Users.Add(createdUser);

        await _db.SaveChangesAsync();
        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", new CancellationToken());
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");

        // Act
        var response = await client.GetFromJsonAsync<AccountModel[]>("/accounts");

        // Assert
        response.Should().NotBeNull();
        response.Should().ContainEquivalentOf(new AccountModel()
        {
            Amount = account.Amount,
            UserId = createdUser.Id,
            Currency = account.Currency,
            DateOfOpening = account.DateOfOpening,
            Number = account.Number
        });
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Accounts.RemoveRange(_db.Accounts);
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Roles.RemoveRange(_db.Roles);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}

public class GetAccountsValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private GetAccounts.RequestValidator _validator;

    public GetAccountsValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory, MemberData(nameof(RandomGuidMock.Guids), MemberType = typeof(RandomGuidMock))]
    public async Task Should_require_user(Guid userId)
    {
        var result = await _validator.TestValidateAsync(new GetAccounts.Request(userId));
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode("general_validation_user_not_exist");
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new GetAccounts.RequestValidator(_db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Accounts.RemoveRange(_db.Accounts);
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Roles.RemoveRange(_db.Roles);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}