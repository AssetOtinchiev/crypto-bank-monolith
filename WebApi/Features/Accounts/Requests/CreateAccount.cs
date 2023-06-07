using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Accounts.Options;

using static WebApi.Features.Accounts.Errors.Codes.AccountsValidationErrors;

namespace WebApi.Features.Accounts.Requests;

public class CreateAccount
{
    public record Request(Guid UserId, string Currency, decimal Amount) : IRequest<Response>;

    public record Response(AccountModel AccountModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext, IOptions<AccountsOptions> accountsOptions)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithErrorCode(AmountLow);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithErrorCode(CurrencyRequired)
                .MinimumLength(3)
                .WithErrorCode(CurrencyTooShort);

            RuleFor(x => x.UserId)
                .NotEmpty()
                .MustAsync(async (x, token) =>
                {
                    var userExists = await dbContext.Users.AnyAsync(user => user.Id == x, token);

                    return userExists;
                }).WithErrorCode(UserNotExist);


            RuleFor(x => x.UserId)
                .MustAsync(async (x, token) =>
                {
                    var accountCount = await dbContext.Accounts
                        .Where(account => account.UserId == x)
                        .CountAsync(token);

                    if (accountCount == accountsOptions.Value.MaxAvailableAccounts)
                    {
                        return false;
                    }

                    return true;
                }).WithErrorCode(LimitExceeded);
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
            var account = new Account()
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                DateOfOpening = DateTime.Now.ToUniversalTime()
            };
            _dbContext.Accounts.Add(account);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(new AccountModel
            {
                Number = account.Number,
                Amount = account.Amount,
                Currency = account.Currency,
                UserId = account.UserId,
                DateOfOpening = account.DateOfOpening
            });
        }
    }
}