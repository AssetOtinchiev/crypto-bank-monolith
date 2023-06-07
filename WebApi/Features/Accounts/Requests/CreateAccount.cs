using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Errors.Exceptions;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Accounts.Options;
using WebApi.Validations;
using static WebApi.Features.Accounts.Errors.Codes.AccountValidationErrors;
using static WebApi.Features.Accounts.Errors.Codes.AccountLogicConflictErrors;

namespace WebApi.Features.Accounts.Requests;

public class CreateAccount
{
    public record Request(Guid UserId, string Currency, decimal Amount) : IRequest<Response>;

    public record Response(AccountModel AccountModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
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
                .UserExist(dbContext);
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;
        private readonly AccountsOptions _accountsOptions;

        public RequestHandler(AppDbContext dbContext, IOptions<AccountsOptions> accountsOptions)
        {
            _dbContext = dbContext;
            _accountsOptions = accountsOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var accountCount = await _dbContext.Accounts
                .Where(account => account.UserId == request.UserId)
                .CountAsync(cancellationToken);

            if (accountCount == _accountsOptions.MaxAvailableAccounts)
            {
                throw new LogicConflictException("Accounts limit exceeded", LimitExceeded);
            }
            
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