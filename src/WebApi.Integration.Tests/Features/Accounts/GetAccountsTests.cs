// using System.Net;
// using System.Net.Http.Json;
// using FluentAssertions;
// using FluentValidation.TestHelper;
// using Microsoft.Extensions.DependencyInjection;
// using WebApi.Database;
// using WebApi.Features.Accounts.Domain;
// using WebApi.Features.Accounts.Models;
// using WebApi.Features.Accounts.Requests;
// using WebApi.Features.Auth.Services;
// using WebApi.Features.Users.Domain;
// using WebApi.Integration.Tests.Features.Users.MockData;
// using WebApi.Integration.Tests.Helpers;
//
// namespace WebApi.Integration.Tests.Features.Accounts;
//
// public class GetAccountsTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
// {
//     private readonly TestingWebAppFactory<Program> _factory;
//     private AppDbContext _db;
//     private AsyncServiceScope _scope;
//     private CancellationToken _cancellationToken;
//
//     public GetAccountsTests(TestingWebAppFactory<Program> factory)
//     {
//         _factory = factory;
//     }
//
//     [Fact]
//     public async Task Should_get_accounts()
//     {
//         // Arrange
//         var client = _factory.CreateClient();
//
//         var createdUser = CreateUserHelper.CreateUser("test@gmail.com", RoleType.User);
//         var account = new Account()
//         {
//             Amount = 10,
//             Currency = "btc",
//             DateOfOpening = DateTime.Now.ToUniversalTime()
//         };
//         var fakeUser = AddFakeUser();
//
//         createdUser.Accounts.Add(account);
//         _db.Users.Add(createdUser);
//         _db.Users.Add(fakeUser);
//
//         await _db.SaveChangesAsync(_cancellationToken);
//         await CreateUserHelper.FillAuthToken(client, _scope, createdUser, _cancellationToken);
//
//         // Act
//         var response =
//             await client.GetFromJsonAsync<AccountModel[]>("/accounts", cancellationToken: _cancellationToken);
//
//         // Assert
//         response.Should().NotBeEmpty();
//         response.Should().ContainEquivalentOf(new AccountModel()
//         {
//             Amount = account.Amount,
//             UserId = createdUser.Id,
//             Currency = account.Currency,
//             DateOfOpening = account.DateOfOpening,
//             Number = account.Number
//         });
//     }
//
//     [Fact]
//     public async Task Should_validate_auth_token()
//     {
//         // Arrange
//         var client = _factory.CreateClient();
//         client.DefaultRequestHeaders.Add("Authorization",
//             $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiJhYmNkMTIzIiwiZXhwaXJ5IjoxNjQ2NjM1NjExMzAxfQ.");
//
//         // Act
//         var response = await client.GetAsync("/accounts", cancellationToken: _cancellationToken);
//
//         // Assert
//         response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
//     }
//
//     private static User AddFakeUser()
//     {
//         var fakeAccount = new Account()
//         {
//             Amount = 11,
//             Currency = "eth",
//             DateOfOpening = DateTime.Now.ToUniversalTime()
//         };
//         var createdFakeUser = CreateUserHelper.CreateUser("testFake@gmail.com", RoleType.User);
//         createdFakeUser.Accounts.Add(fakeAccount);
//         return createdFakeUser;
//     }
//
//     public Task InitializeAsync()
//     {
//         BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
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
//
// public class GetAccountsValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
// {
//     private readonly TestingWebAppFactory<Program> _factory;
//     private AppDbContext _db;
//     private AsyncServiceScope _scope;
//     private GetAccounts.RequestValidator _validator;
//     private CancellationToken _cancellationToken;
//
//     public GetAccountsValidatorTests(TestingWebAppFactory<Program> factory)
//     {
//         _factory = factory;
//     }
//
//     [Theory, MemberData(nameof(RandomGuidMock.Guids), MemberType = typeof(RandomGuidMock))]
//     public async Task Should_require_user(Guid userId)
//     {
//         var result =
//             await _validator.TestValidateAsync(new GetAccounts.Request(userId), cancellationToken: _cancellationToken);
//         result.ShouldHaveValidationErrorFor(x => x.UserId)
//             .WithErrorCode("general_validation_user_not_exist");
//     }
//
//     public Task InitializeAsync()
//     {
//          BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
//         _validator = new GetAccounts.RequestValidator(_db);
//
//         return Task.CompletedTask;
//     }
//
//     public async Task DisposeAsync()
//     {
//         BaseInitializeHelper.DisposeDatabase(ref _db);
//         await _db.SaveChangesAsync();
//         await _scope.DisposeAsync();
//     }
// }