using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using WebApi.Database;
using WebApi.Errors.Exceptions;
using WebApi.Features.Deposits.Domain;
using static WebApi.Features.Users.Errors.Codes.UserValidationErrors;
using static WebApi.Features.Deposits.Errors.Codes.DepositValidationErrors;

namespace WebApi.Features.Deposits.Requests;

public class GetDepositAddress
{
    public record Request(Guid UserId, string CurrencyCode) : IRequest<Response>;

    public record Response(string CryptoAddress);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
        {
            RuleFor(x => x.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MustAsync(async (x, token) =>
                {
                    var userExists = await dbContext.Users.AnyAsync(user => user.Id == x, token);

                    return userExists;
                }).WithErrorCode(NotExist);

            RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .MinimumLength(3)
                .Equal("BTC") //todo create table with currencies
                .WithErrorCode(InvalidCurrencyCode);
            
        }
    }
    
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var xpub = await _dbContext.Xpubs
                .SingleOrDefaultAsync(x => x.CurrencyCode == request.CurrencyCode, cancellationToken: cancellationToken);

            if (xpub is null)
            {
                throw new LogicConflictException("CurrencyCode not exist", XPubCurrencyCodeNotExist);
            }

            var depositAddress = await _dbContext.DepositAddresses
                .SingleOrDefaultAsync(x => x.UserId == request.UserId && x.XpubId == xpub.Id, 
                    cancellationToken: cancellationToken);

            if (depositAddress != null)
            {
                return new Response(depositAddress.CryptoAddress);
            }
            
            var lastDerivationIndex = await GetLastDerivationIndex(cancellationToken, xpub);
            var bitcoinAddress = GenerateAddress(xpub, lastDerivationIndex);

            await _dbContext.DepositAddresses.AddAsync(new DepositAddress()
            {
                CryptoAddress = bitcoinAddress.ToString(),
                CurrencyCode = request.CurrencyCode,
                DerivationIndex = lastDerivationIndex,
                UserId = request.UserId,
                XpubId = xpub.Id
            }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(bitcoinAddress.ToString());
        }

        private static BitcoinAddress GenerateAddress(Xpub xpub, int lastDerivationIndex)
        {
            var pubkey = ExtPubKey.Parse(xpub.Value, Network.TestNet);
            var bitcoinAddress = pubkey.Derive(0).Derive(Convert.ToUInt32(lastDerivationIndex)).PubKey
                .GetAddress(ScriptPubKeyType.Segwit, Network.TestNet);
            return bitcoinAddress;
        }

        private async Task<int> GetLastDerivationIndex(CancellationToken cancellationToken, Xpub xpub)
        {
            var lastDerivationIndex =
                (await _dbContext.DepositAddresses.AsNoTracking().CountAsync(x => x.XpubId == xpub.Id, cancellationToken: cancellationToken))
                + 1;
            return lastDerivationIndex;
        }
    }
}