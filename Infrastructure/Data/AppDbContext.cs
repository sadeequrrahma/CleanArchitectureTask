using CleanArchitectureTask.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTask.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(u => !u.IsDeleted);

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
            entity.Property(u => u.ProfileImageBlobPath).HasMaxLength(512);

            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.CreatedAt);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(2000).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => new { p.Price, p.Name });
        });
    }
}
