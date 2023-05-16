using Domain.Entitites;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces.Infrastructure;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}