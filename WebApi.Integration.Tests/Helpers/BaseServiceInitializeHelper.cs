using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;

namespace WebApi.Integration.Tests.Helpers;

public class BaseServiceInitializeHelper
{
    public void Initialize(TestingWebAppFactory<Program> factory, ref AsyncServiceScope scope,
        ref AppDbContext dbContext, ref CancellationToken cancellationToken)
    {
        scope = factory.Services.CreateAsyncScope();
        dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        cancellationToken = new CancellationTokenHelper().GetCancellationToken();
    }
}