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
using WebApi.Features.Users.Requests;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Accounts;

public class GetAccountOpenedByPeriodTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    public GetAccountOpenedByPeriodTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory, MemberData(nameof(Accounts))]
    public async Task Should_get_account_report(Account[] accounts)
    {
        // Arrange
        var client = _factory.CreateClient();

        var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
        var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken, RoleType.Analyst);
        
        createdUser.Accounts.AddRange(accounts);
        await _db.SaveChangesAsync(_cancellationToken);
        
        await CreateUserHelper.FillAuthToken(client, _scope, createdUser, _cancellationToken);

        var startDate = DateTimeOffset.Now.AddDays(-2).ToUniversalTime();
        var endDate = DateTimeOffset.Now.ToUniversalTime();
        var query = new Dictionary<string, string>
        {
            ["startDate"] = startDate.ToString(),
            ["endDate"] = endDate.ToString()
        };

        // Act
        var response =
            await client.GetFromJsonAsync<GetAccountOpenedByPeriodModel[]>(
                QueryHelpers.AddQueryString("/accounts/period", query), cancellationToken: _cancellationToken);

        // Assert
        response.Should().NotBeEmpty();

        response.Length.Should().Be(2);
        var filteredAccounts =
            accounts.Where(x => x.DateOfOpening >= startDate && x.DateOfOpening <= endDate).ToArray();
        foreach (var account in filteredAccounts)
        {
            var periodData = response.FirstOrDefault(x => x.Date == account.DateOfOpening.Date);
            periodData.Should().NotBeNull();
            periodData.Count.Should().Be(1);
        }
    }

    [Theory, MemberData(nameof(Accounts))]
    public async Task Should_require_user_role(Account[] accounts)
    {
        // Arrange
        var client = _factory.CreateClient();

        var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
        var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);

        await _db.SaveChangesAsync();
        await CreateUserHelper.FillAuthToken(client, _scope, createdUser, _cancellationToken);

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

    [Fact]
    public async Task Should_validate_auth_token()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization",
            $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiJhYmNkMTIzIiwiZXhwaXJ5IjoxNjQ2NjM1NjExMzAxfQ.");

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
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
                    },
                    new()
                    {
                        Amount = 12,
                        Currency = "eth",
                        DateOfOpening = DateTime.Now.AddDays(-20).ToUniversalTime()
                    },
                    new()
                    {
                        Amount = 12,
                        Currency = "eth",
                        DateOfOpening = DateTime.Now.AddDays(20).ToUniversalTime()
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
                    },
                    new()
                    {
                        Amount = 12,
                        Currency = "eth",
                        DateOfOpening = DateTime.Now.AddDays(-20).ToUniversalTime()
                    },
                    new()
                    {
                        Amount = 12,
                        Currency = "eth",
                        DateOfOpening = DateTime.Now.AddDays(20).ToUniversalTime()
                    }
                }
            };
        }
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