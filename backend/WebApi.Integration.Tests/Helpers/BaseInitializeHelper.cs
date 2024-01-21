// using Microsoft.Extensions.DependencyInjection;
// using WebApi.Database;
//
// namespace WebApi.Integration.Tests.Helpers;
//
// public static class BaseInitializeHelper
// {
//     public static void Initialize(TestingWebAppFactory<Program> factory, ref AsyncServiceScope scope,
//         ref AppDbContext dbContext, ref CancellationToken cancellationToken)
//     {
//         scope = factory.Services.CreateAsyncScope();
//         dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//         cancellationToken =  CancellationTokenHelper.GetCancellationToken();
//     }
//
//     public static void DisposeDatabase(ref AppDbContext dbContext)
//     {
//         dbContext.Accounts.RemoveRange(dbContext.Accounts);
//         dbContext.RefreshTokens.RemoveRange(dbContext.RefreshTokens);
//         dbContext.Roles.RemoveRange(dbContext.Roles);
//         dbContext.Users.RemoveRange(dbContext.Users);
//     }
// }