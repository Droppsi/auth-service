using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(x => x.Id);

        modelBuilder.Entity<User>().Property(x => x.Username)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<User>().Property(x => x.Password)
            .HasMaxLength(72)
            .IsRequired();

        modelBuilder.Entity<User>().Property(x => x.RefreshToken)
            .HasMaxLength(255)
            .IsRequired(false);
    }
}