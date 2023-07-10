using Microsoft.EntityFrameworkCore;
using NBitcoin;
using WebApi.Database;
using WebApi.Features.Deposits.Domain;

namespace WebApi.Jobs;

public class XPubInitializer : IHostedService
{
    private readonly ILogger<XPubInitializer> _logger;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private const string CurrencyCode = "BTC";

    public XPubInitializer(ILogger<XPubInitializer> logger, IDbContextFactory<AppDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var xpubExists =
            await context.Xpubs.AnyAsync(x => x.CurrencyCode == CurrencyCode, cancellationToken: cancellationToken);

        if (xpubExists)
            return;

        var masterPubKey = GeneratePubKey();

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

    /// <summary>
    /// https://programmingblockchain.gitbook.io/programmingblockchain/key_generation/bip_32
    /// </summary>
    /// <returns></returns>
    private string GeneratePubKey()
    {
        var masterPrvKey = new ExtKey();
        var network = Network.TestNet;
        _logger.LogInformation($"Master private key: [{masterPrvKey.ToString(network)}]");

        var masterPubKey = masterPrvKey.Neuter();
        var masterPubKeyAsString = masterPubKey.ToString(network);
        _logger.LogInformation($"Master public key: [{masterPubKeyAsString}]");
        return masterPubKeyAsString;
    }
}