using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Options;
using WebApi.Features.Accounts.Requests;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Requests;
using WebApi.Integration.Tests.Common;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Fixtures;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Accounts;

[Collection(AccountTestsCollection.Name)]
public class CreateAccountTests :IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly HttpClientHarness<Program> _httpClient;
    private AccountsOptions _accountsOptions;

    public CreateAccountTests(TestFixture testFixture)
    {
        _database = testFixture.Database;
        _httpClient = testFixture.HttpClient;
        _factory= testFixture.Factory;
        
         using var scope = _factory.Services.CreateScope();
        _accountsOptions = scope.ServiceProvider.GetRequiredService<IOptions<AccountsOptions>>().Value;
    }

    [Fact]
    public async Task Should_create_account()
    {
        // Arrange
        var client = _httpClient.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
        var createdUser = await CreateUserHelper.CreateUser(userRequest, scope, CancellationTokenHelper.GetCancellationToken());
        await CreateUserHelper.FillAuthToken(client, scope, createdUser, CancellationTokenHelper.GetCancellationToken());
        var amount = 100;
        var currency = "btc";

        // Act
        (await client.PostAsJsonAsync("/accounts", new
        {
            UserId = createdUser.Id,
            Currency = currency,
            Amount = amount,
        }, cancellationToken: CancellationTokenHelper.GetCancellationToken())).EnsureSuccessStatusCode();

        // Assert
        var account = await _database
            .SingleOrDefault<Account>(x => x.UserId == createdUser.Id, cancellationToken: CancellationTokenHelper.GetCancellationToken());

        account.Should().NotBeNull();
        account.Amount.Should().Be(amount);
        account.Currency.Should().Be(currency);
    }

    // [Fact]
    // public async Task Should_return_logic_error_if_account_limit_exceeded()
    // {
    //     // Arrange
    //     var client = _factory.CreateClient();
    //
    //     var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
    //     var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);
    //
    //     var accounts = new List<Account>();
    //     for (int i = 0; i < _accountsOptions.MaxAvailableAccounts; i++)
    //     {
    //         accounts.Add(new Account()
    //         {
    //             UserId = createdUser.Id,
    //             Currency = Guid.NewGuid().ToString(),
    //             Amount = new Random().Next(1, 15),
    //             DateOfOpening = DateTime.Now.ToUniversalTime()
    //         });
    //     }
    //
    //     _db.Accounts.AddRange(accounts);
    //     await _db.SaveChangesAsync(_cancellationToken);
    //
    //     await CreateUserHelper.FillAuthToken(client, _scope, createdUser, _cancellationToken);
    //
    //     var amount = 100;
    //     var currency = "btc";
    //
    //     // Act
    //     var result = await client.PostAsJsonAsync("/accounts", new
    //     {
    //         UserId = createdUser.Id,
    //         Currency = currency,
    //         Amount = amount,
    //     }, cancellationToken: _cancellationToken);
    //
    //     var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
    //
    //     // Assert
    //     result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    //     response.Detail.Should().Be("Accounts limit exceeded");
    // }
    //
    // [Fact]
    // public async Task Should_validate_auth_token()
    // {
    //     // Arrange
    //     var client = _factory.CreateClient();
    //     client.DefaultRequestHeaders.Add("Authorization",
    //         $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiJhYmNkMTIzIiwiZXhwaXJ5IjoxNjQ2NjM1NjExMzAxfQ.");
    //
    //     // Act
    //     var result = await client.PostAsJsonAsync("/accounts", new
    //     {
    //         UserId = 1,
    //         Currency = "btc",
    //         Amount = 1,
    //     }, cancellationToken: CancellationTokenHelper.GetCancellationToken());
    //
    //     // Assert
    //     result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    // }

    // public Task InitializeAsync()
    // {
    //     BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
    //     _accountsOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AccountsOptions>>().Value;
    //     return Task.CompletedTask;
    // }

    public async Task InitializeAsync()
    {
        await _database.Clear(CancellationTokenHelper.GetCancellationToken());
    }
        

    public async Task DisposeAsync()
    {
        // BaseInitializeHelper.DisposeDatabase(ref _db);
        // await _db.SaveChangesAsync(_cancellationToken);
        // await _scope.DisposeAsync();
        return;
    }
}


//
// public class CreateAccountValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
// {
//     private readonly TestingWebAppFactory<Program> _factory;
//     private AppDbContext _db;
//     private AsyncServiceScope _scope;
//     private CreateAccount.RequestValidator _validator;
//     private CancellationToken _cancellationToken;
//
//     public CreateAccountValidatorTests(TestingWebAppFactory<Program> factory)
//     {
//         _factory = factory;
//     }
//
//     [Fact]
//     public async Task Should_require_user()
//     {
//         var result = await _validator.TestValidateAsync(new CreateAccount.Request(Guid.NewGuid(), "btc", 1));
//         result.ShouldHaveValidationErrorFor(x => x.UserId)
//             .WithErrorCode("general_validation_user_not_exist");
//     }
//
//     [Theory]
//     [InlineData("")]
//     [InlineData(" ")]
//     public async Task Should_require_currency(string currency)
//     {
//         var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
//         var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);
//
//         var result = await _validator.TestValidateAsync(new CreateAccount.Request(createdUser.Id, currency, 1),
//             cancellationToken: _cancellationToken);
//         result.ShouldHaveValidationErrorFor(x => x.Currency)
//             .WithErrorCode("accounts_validation_currency_required");
//     }
//
//     [Theory]
//     [InlineData("aa")]
//     [InlineData("a")]
//     public async Task Should_validate_currency_length(string currency)
//     {
//         var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
//         var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);
//
//         var result = await _validator.TestValidateAsync(new CreateAccount.Request(createdUser.Id, currency, 1),
//             cancellationToken: _cancellationToken);
//         result.ShouldHaveValidationErrorFor(x => x.Currency)
//             .WithErrorCode("accounts_validation_currency_too_short");
//     }
//
//     [Theory]
//     [InlineData(-222)]
//     [InlineData(-1)]
//     public async Task Should_validate_amount(decimal amount)
//     {
//         var userRequest = new RegisterUser.Request("test@gmail.com", "aaaAAAaa", DateTime.Now.ToUniversalTime());
//         var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);
//
//         var result = await _validator.TestValidateAsync(new CreateAccount.Request(createdUser.Id, "btc", amount),
//             cancellationToken: _cancellationToken);
//         result.ShouldHaveValidationErrorFor(x => x.Amount)
//             .WithErrorCode("accounts_validation_amount_low");
//     }
//
//     public Task InitializeAsync()
//     {
//         BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
//         _validator = new CreateAccount.RequestValidator(_db);
//
//         return Task.CompletedTask;
//     }
//
//     public async Task DisposeAsync()
//     {
//         BaseInitializeHelper.DisposeDatabase(ref _db);
//         await _db.SaveChangesAsync(_cancellationToken);
//         await _scope.DisposeAsync();
//     }
// }