using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Models;

namespace WebApi.Features.Users.Requests;

public class EditUserRoles
{
    public record Request(RoleType[] Roles, Guid UserId) : IRequest<Response>;

    public record Response(RoleModel[] UserModel);

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

            RuleFor(x => x.Roles)
                .NotNull()
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
            var roles = new List<Role>();
            foreach (var requestRole in request.Roles)
            {
                roles.Add(new Role()
                {
                    Name = requestRole,
                    UserId = request.UserId
                });
            }

            var userRoles = await _dbContext.Roles.Where(x => x.UserId == request.UserId)
                .ToArrayAsync(cancellationToken: cancellationToken);

            _dbContext.Roles.RemoveRange(userRoles);
            _dbContext.Roles.AddRange(roles);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(roles.Select(x => new RoleModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToArray());
        }
    }
}