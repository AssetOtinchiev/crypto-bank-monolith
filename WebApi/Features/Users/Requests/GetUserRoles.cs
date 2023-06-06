using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Users.Models;

using static WebApi.Features.Users.Errors.Codes.UserValidationErrors;

namespace WebApi.Features.Users.Requests;

public class GetUserRoles
{
    public record Request(Guid UserId) : IRequest<Response>;

    public record Response(RoleModel[] RoleModels);

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
                }).WithErrorCode(UserNotExist);
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
            var roleModels = await _dbContext.Roles.Where(x=> x.UserId == request.UserId)
                .Select(x => new RoleModel
                {
                    UserId = x.UserId,
                    Name = x.Name
                }).ToArrayAsync();
            
            return new Response(roleModels);
        }
    }
}