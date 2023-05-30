using Microsoft.EntityFrameworkCore;
using WebApi.Features.Accounts.Domain;
using WebApi.Features.Auth.Domain;
using WebApi.Features.Users.Domain;

namespace WebApi.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext()
    {

    }

    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    
    public virtual DbSet<Account> Accounts { get; set; } 
    
    public virtual DbSet<Role> Roles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapRefreshTokens(modelBuilder);
        MapAccounts(modelBuilder);
        MapUsers(modelBuilder);
        MapRoles(modelBuilder);
    }

    private void MapRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(role =>
        {
            role.HasKey(x => new {x.UserId, x.Name});
            
            role.Property(e => e.UserId)
                .IsRequired();
            
            role.Property(e => e.Name)
                .IsRequired();
            
            role.Property(e => e.CreatedAt)
                .IsRequired();
        });
    }

    private void MapUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(x => x.Id);

            user.Property(e => e.Email)
                .IsRequired();
            
            user.Property(e => e.Password)
                .IsRequired();

            user.Property(e => e.DateOfBirth)
                .IsRequired();
            
            user.Property(e => e.RegisteredAt)
                .IsRequired();
        });
    }

    private static void MapRefreshTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(refreshToken =>
        {
            refreshToken.HasKey(x => x.Id);
            
            refreshToken.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(1000);

            refreshToken.Property(e => e.TokenSalt)
                .IsRequired()
                .HasMaxLength(1000);
            
            refreshToken.Property(e => e.UserId)
                .IsRequired();

            refreshToken.HasOne(d => d.User)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
    
    private static void MapAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(x => x.Number);
            
            account.Property(e => e.Amount)
                .IsRequired();
            
            account.Property(e => e.Currency)
                .IsRequired();
            
            account.Property(e => e.DateOfOpening)
                .IsRequired();
            
            account.Property(e => e.UserId)
                .IsRequired();

            account.HasOne(d => d.User)
                .WithMany(p => p.Accounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
}