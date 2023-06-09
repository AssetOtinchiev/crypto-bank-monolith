using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Integration.Tests.Features.Users.MockData;

namespace WebApi.Integration.Tests.Features.Accounts;

public class GetAccountOpenedByPeriodTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;

    public GetAccountOpenedByPeriodTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory, MemberData(nameof(Accounts))]
    public async Task Should_get_account_report(Account[] accounts)
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.Analyst);
        createdUser.Accounts.AddRange(accounts);
        _db.Users.Add(createdUser);

        await _db.SaveChangesAsync();
        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", new CancellationToken());
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");

        var query = new Dictionary<string, string>
        {
            ["startDate"] = DateTime.Now.AddDays(-2).ToUniversalTime().ToString(CultureInfo.InvariantCulture),
            ["endDate"] = DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)
        };

        // Act
        var response =
            await client.GetFromJsonAsync<GetAccountOpenedByPeriodModel[]>(
                QueryHelpers.AddQueryString("/accounts/period", query));

        // Assert
        response.Should().NotBeNull();
        response.Length.Should().BeGreaterThan(1);
    }

    [Theory, MemberData(nameof(Accounts))]
    public async Task Should_require_user_role(Account[] accounts)
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        createdUser.Accounts.AddRange(accounts);
        _db.Users.Add(createdUser);

        await _db.SaveChangesAsync();
        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", new CancellationToken());
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");

        var query = new Dictionary<string, string>
        {
            ["startDate"] = DateTime.Now.AddDays(-2).ToUniversalTime().ToString(CultureInfo.InvariantCulture),
            ["endDate"] = DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)
        };

        // Act
        var response =
            await client.GetAsync(
                QueryHelpers.AddQueryString("/accounts/period", query));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static IEnumerable<object[]> Accounts
    {
        get
        {
            yield return new object[]
            {
                new List<Account>()
                {
                    new()
                    {
                        Amount = 15,
                        Currency = "eth",
                        DateOfOpening = DateTime.Now.AddDays(-1).ToUniversalTime()
                    },
                    new()
                    {
                        Amount = 10,
                        Currency = "btc",
                        DateOfOpening = DateTime.Now.ToUniversalTime()
                    }
                }
            };
            yield return new object[]
            {
                new List<Account>()
                {
                    new()
                    {
                        Amount = 16,
                        Currency = "ada",
                        DateOfOpening = DateTime.Now.AddDays(-1).ToUniversalTime()
                    },
                    new()
                    {
                        Amount = 15,
                        Currency = "xrp",
                        DateOfOpening = DateTime.Now.ToUniversalTime()
                    }
                }
            };
        }
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