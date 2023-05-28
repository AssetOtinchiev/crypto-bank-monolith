using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Accounts.Options;

namespace WebApi.Features.Accounts.Requests;

public class CreateAccount
{
    public record Request(CreateAccountModel CreateAccountModel) : IRequest<Response>;

    public record Response(AccountModel AccountModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext, IOptions<AccountsOptions> accountsOptions)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.CreateAccountModel.Amount)
                .GreaterThan(0);

            RuleFor(x => x.CreateAccountModel.Currency)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.CreateAccountModel.UserId)
                .NotEmpty()
                .MustAsync(async (x, token) =>
                {
                    var isExistUser = await dbContext.Users.AnyAsync(user => user.Id == x, token);

                    return isExistUser;
                }).WithMessage("User not exists in database");


            RuleFor(x => x.CreateAccountModel.UserId)
                .MustAsync(async (x, token) =>
                {
                    var accountCount = await dbContext.Accounts
                        .Where(account => account.UserId == x)
                        .CountAsync();

                    if (accountCount == accountsOptions.Value.MaxAvailableAccounts)
                    {
                        return false;
                    }

                    return true;
                }).WithMessage("Account limit exceeded");
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
                UserId = request.CreateAccountModel.UserId,
                Amount = request.CreateAccountModel.Amount,
                Currency = request.CreateAccountModel.Currency
            };
            _dbContext.Accounts.Add(account);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(new AccountModel
            {
                Id = account.Id,
                Amount = account.Amount,
                Currency = account.Currency,
                UserId = account.UserId,
                DateOfOpening = account.DateOfOpening
            });
        }
    }
}