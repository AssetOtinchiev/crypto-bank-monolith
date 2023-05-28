using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;

namespace WebApi.Features.Accounts.Requests;

public class GetAccountOpenedByPeriod
{
    public record Request(DateTime StartDate, DateTime EndDate) : IRequest<Response>;

    public record Response(int Count);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.StartDate)
                .NotEmpty();

            RuleFor(x => x.EndDate)
                .NotEmpty();
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
            var accountCount = await _dbContext.Accounts
                .Where(x => x.DateOfOpening >= request.StartDate)
                .Where(x => x.DateOfOpening <= request.EndDate)
                .CountAsync(cancellationToken: cancellationToken);

            return new Response(accountCount);
        }
    }
}