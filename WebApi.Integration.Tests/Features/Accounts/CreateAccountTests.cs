using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Accounts.Requests;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Accounts;

public class CreateAccountTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

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

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
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