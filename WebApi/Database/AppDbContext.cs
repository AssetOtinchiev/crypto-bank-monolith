using Microsoft.EntityFrameworkCore;
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
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {

            entity.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.TokenSalt)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasOne(d => d.User)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_User");
        });
    }
}