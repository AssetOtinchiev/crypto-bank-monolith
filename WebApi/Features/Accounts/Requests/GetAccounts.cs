using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Accounts.Models;

using static WebApi.Features.Accounts.Errors.Codes.AccountsValidationErrors;
    
namespace WebApi.Features.Accounts.Requests;

public class GetAccounts
{
    public record Request(Guid UserId) : IRequest<Response>;

    public record Response(AccountModel[] AccountModels);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.UserId)
                .NotEmpty()
                .MustAsync(async (x, token) =>
                {
                    var userExists = await dbContext.Users.AnyAsync(user => user.Id == x, token);

                    return userExists;
                }).WithErrorCode(AccountsUserNotExist);
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
            var accounts = await _dbContext.Accounts
                .Where(x => x.UserId == request.UserId)
                .Select(x => new AccountModel()
                {
                    Number = x.Number,
                    UserId = x.UserId,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DateOfOpening = x.DateOfOpening
                })
                .ToArrayAsync(cancellationToken);

            return new Response(accounts);
        }
    }
}