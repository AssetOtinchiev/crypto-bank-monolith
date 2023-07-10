using WebApi.Jobs;

namespace WebApi.Features.Deposits.Registration;

public static class DepositBuilderExtension
{
    public static WebApplicationBuilder AddDeposits(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<XPubInitializer>();
        return builder;
    }
}