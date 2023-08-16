using WebApi.Features.Deposits.Options;
using WebApi.Jobs;

namespace WebApi.Features.Deposits.Registration;

public static class DepositBuilderExtension
{
    public static WebApplicationBuilder AddDeposits(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DepositOptions>(builder.Configuration.GetSection("Features:Deposits"));
        builder.Services.AddHostedService<XPubInitializer>();
        return builder;
    }
}