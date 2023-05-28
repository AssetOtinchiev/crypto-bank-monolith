using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Accounts.Options;

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
                    var isExistUser = await dbContext.Users.AnyAsync(user => user.Id == x, token);

                    return isExistUser;
                }).WithMessage("User not exists in database");
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
                    Id = x.Id,
                    UserId = x.UserId,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DateOfOpening = x.DateOfOpening
                })
                .ToArrayAsync(cancellationToken: cancellationToken);

            return new Response(accounts);
        }
    }
}