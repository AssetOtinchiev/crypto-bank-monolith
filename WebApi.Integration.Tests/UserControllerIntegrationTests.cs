// using System.Net;
// using System.Net.Http.Json;
// using Contracts.Dtos;
// using Domain.Entitites;
// using Infrastructure.Persistence.Context;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace WebApi.Integration.Tests;
//
// public class UserControllerIntegrationTests : IClassFixture<TestingWebAppFactory<Program>>
// {
//     private readonly TestingWebAppFactory<Program> _factory;
//
//     public UserControllerIntegrationTests(TestingWebAppFactory<Program> factory)
//     {
//         _factory = factory;
//     }
//
//     [Fact]
//     public async Task Register_WhenUserNotExistInDb_ShouldReturnUser()
//     {
//         var request = new CreateUserDto
//         {
//             Email = "123123@gmail.com",
//             Password = "asdasdsaAA"
//         };
//
//         var response = await _client.PostAsJsonAsync("/register", request);
//
//         response.EnsureSuccessStatusCode();
//
//         var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
//
//         Assert.Equal(request.Email, userDto.Email);
//     }
//
//     [Fact]
//     public async Task Register_WhenUserExist_ShouldReturnBadRequest()
//     {
//         var scope = _factory.Services.CreateScope();
//         var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//
//         await database.Users.AddAsync(new User()
//         {
//             Id = Guid.NewGuid(),
//             Email = "123123@gmail.com",
//             Password = "1",
//             PasswordSalt = "1"
//         });
//         await database.SaveChangesAsync();
//
//         var request = new CreateUserDto
//         {
//             Email = "123123@gmail.com",
//             Password = "asdasdsaAA"
//         };
//
//         var response = await _client.PostAsJsonAsync("/register", request);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//     }
// }