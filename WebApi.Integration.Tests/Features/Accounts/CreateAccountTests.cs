using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApi.Database;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Options;
using WebApi.Features.Accounts.Requests;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Options;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Accounts;

public class CreateAccountTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private AccountsOptions _accountsOptions;

    public CreateAccountTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_create_account()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", _cancellationToken);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");
        var amount = 100;
        var currency = "btc";
        
        // Act
        (await client.PostAsJsonAsync("/accounts", new
        {
            UserId = createdUser.Id,
            Currency = currency,
            Amount = amount,
        }, cancellationToken: _cancellationToken)).EnsureSuccessStatusCode();

        // Assert
        var account = await _db.Accounts
            .SingleOrDefaultAsync(x => x.UserId == createdUser.Id, cancellationToken: _cancellationToken);

        account.Should().NotBeNull();
        account.Amount.Should().Be(amount);
        account.Currency.Should().Be(currency);
    }

    [Fact]
    public async Task Should_return_logic_error_if_account_limit_exceeded()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var accounts = new List<Account>();
        for (int i = 0; i < _accountsOptions.MaxAvailableAccounts; i++)
        {
            accounts.Add(new Account()
            {
                UserId = createdUser.Id,
                Currency = Guid.NewGuid().ToString(),
                Amount = new Random().Next(1,15),
                DateOfOpening = DateTime.Now.ToUniversalTime()
            });
        }
        _db.Accounts.AddRange(accounts);
        await _db.SaveChangesAsync(_cancellationToken);

        var tokenService = _scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "test", _cancellationToken);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");
        var amount = 100;
        var currency = "btc";
        
        // Act
        var result = await client.PostAsJsonAsync("/accounts", new
        {
            UserId = createdUser.Id,
            Currency = currency,
            Amount = amount,
        }, cancellationToken: _cancellationToken);

        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Detail.Should().Be("Accounts limit exceeded");
    }

    [Fact]
    public async Task Should_validate_auth_token()
    { 
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiJhYmNkMTIzIiwiZXhwaXJ5IjoxNjQ2NjM1NjExMzAxfQ.");
        
        // Act
        var result = await client.PostAsJsonAsync("/accounts", new
        {
            UserId = 1,
            Currency = "btc",
            Amount = 1,
        }, cancellationToken: _cancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        _accountsOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AccountsOptions>>().Value;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Accounts.RemoveRange(_db.Accounts);
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}

public class CreateAccountValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CreateAccount.RequestValidator _validator;
    private CancellationToken _cancellationToken;

    public CreateAccountValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_require_user()
    {
        var result = await _validator.TestValidateAsync(new CreateAccount.Request(Guid.NewGuid(), "btc", 1));
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode("general_validation_user_not_exist");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_currency(string currency)
    {
        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(new CreateAccount.Request(createdUser.Id, currency, 1), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorCode("accounts_validation_currency_required");
    }

    [Theory]
    [InlineData("aa")]
    [InlineData("a")]
    public async Task Should_validate_currency_length(string currency)
    {
        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(new CreateAccount.Request(createdUser.Id, currency, 1), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorCode("accounts_validation_currency_too_short");
    }

    [Theory]
    [InlineData(-222)]
    [InlineData(-1)]
    public async Task Should_validate_amount(decimal amount)
    {
        var createdUser = CreateUserMock.CreateUser("test@gmail.com", RoleType.User);
        _db.Users.Add(createdUser);
        await _db.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(new CreateAccount.Request(createdUser.Id, "btc", amount), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorCode("accounts_validation_amount_low");
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _validator = new CreateAccount.RequestValidator(_db);
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Accounts.RemoveRange(_db.Accounts);
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}