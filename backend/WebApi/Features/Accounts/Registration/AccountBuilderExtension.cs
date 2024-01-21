using WebApi.Features.Accounts.Options;

namespace WebApi.Features.Accounts.Registration;

public static class AccountBuilderExtension
{
    public static WebApplicationBuilder AddAccounts(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AccountsOptions>(builder.Configuration.GetSection("Features:Accounts"));
        return builder;
    }
}