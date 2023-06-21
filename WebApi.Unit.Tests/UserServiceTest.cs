// using Application.Services;
// using Contracts.Dtos;
// using Domain.Entitites;
// using Infrastructure.Persistence.Context;
// using Microsoft.EntityFrameworkCore;
// using Moq;
// using Moq.EntityFrameworkCore;
//
// namespace WebApi.Unit.Tests;
//
// public class UserServiceTest
// {
//     [Fact]
//     public async Task Register_WhenUserNotExistInDb_ShouldReturnUser()
//     {
//         // Arrange
//         var contextMock = new Mock<AppDbContext>();
//         contextMock.Setup<DbSet<User>>(x => x.Users)
//             .ReturnsDbSet(new List<User>());
//
//         var tokenMock = new Mock<TokenService>();
//         UserService account = new UserService(contextMock.Object, tokenMock.Object);
//         var data = new CreateUserDto()
//         {
//             Email = "12312312@gmial.com",
//             Password = "asdasdsaaAAAA"
//         };
//
//         // Act 
//         var result = await account.Register(data, new CancellationToken());
//
//         // Assert
//         Assert.NotNull(result.AsT0);
//     }
//
//     [Fact]
//     public async Task Register_WhenUserExist_ShouldReturnErrorResponse()
//     {
//         // Arrange
//         var contextMock = new Mock<AppDbContext>();
//         contextMock.Setup<DbSet<User>>(x => x.Users)
//             .ReturnsDbSet(new List<User>()
//             {
//                 new()
//                 {
//                     Id = Guid.NewGuid(),
//                     Password = "123",
//                     Email = "12312312@gmial.com",
//                     PasswordSalt = "1231"
//                 }
//             });
//         var tokenMock = new Mock<TokenService>();
//         UserService account = new UserService(contextMock.Object, tokenMock.Object);
//         var data = new CreateUserDto()
//         {
//             Email = "12312312@gmial.com",
//             Password = "asdasdsaaAAAA"
//         };
//
//         // Act 
//         var result = await account.Register(data, new CancellationToken());
//
//         // Assert
//         Assert.NotNull(result.AsT1);
//     }
// }