using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Accounts.Models;

namespace WebApi.Features.Accounts.Requests;

public class GetAccountOpenedByPeriod
{
    public record Request(DateTime StartDate, DateTime EndDate) : IRequest<Response>;

    public record Response(GetAccountOpenedByPeriodModel[] GetAccountOpenedByPeriodModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.StartDate)
                .NotEmpty();

            RuleFor(x => x.EndDate)
                .NotEmpty();

            RuleFor(x => x)
                .Must(x =>
                {
                    if (x.StartDate > x.EndDate)
                    {
                        return false;
                    }
                    return true;
                }).WithMessage("StartDate is more thank endDate");
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
            var groupedAccountCount = await _dbContext.Accounts
                .Where(x => x.DateOfOpening >= request.StartDate)
                .Where(x => x.DateOfOpening <= request.EndDate)
                .GroupBy(x => x.DateOfOpening.Date)
                .Select(x => new GetAccountOpenedByPeriodModel
                {
                    Date = x.Key.Date,
                    Count = x.Count()
                }).ToArrayAsync(cancellationToken);

            return new Response(groupedAccountCount);
        }
    }
}