using Application.Interfaces;
using Application.Interfaces.Infrastructure;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class AppDbContext : DbContext , IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext()
    {

    }
    
    public virtual DbSet<User> Users { get; set; }
}