using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;
using WebApi.Database;
using WebApi.Features.Deposits.Domain;
using WebApi.Features.Deposits.Options;

namespace WebApi.Jobs;

public class XPubInitializer : IHostedService
{
    private readonly ILogger<XPubInitializer> _logger;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly DepositOptions _depositOptions;
    private const string CurrencyCode = "BTC";

    public XPubInitializer(ILogger<XPubInitializer> logger, IDbContextFactory<AppDbContext> contextFactory, IOptions<DepositOptions> depositOptions)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _depositOptions = depositOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var xpubExists =
            await context.Xpubs.AnyAsync(x => x.CurrencyCode == CurrencyCode, cancellationToken: cancellationToken);

        if (xpubExists)
            return;

        var masterPubKey = _depositOptions.XpubValue;

        var xpubEntity = new Xpub
        {
            Value = masterPubKey,
            CurrencyCode = CurrencyCode
        };

        await context.Xpubs.AddAsync(xpubEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}