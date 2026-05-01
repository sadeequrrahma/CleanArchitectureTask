using CleanArchitectureTask.Domain.Entities;
using CleanArchitectureTask.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureTask.Infrastructure.Data;

public static class DbInitializer
{
    private const string AdminEmail = "admin@demo.local";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        await context.Database.MigrateAsync(cancellationToken);

        var adminExists = await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == AdminEmail, cancellationToken);
        if (adminExists)
            return;

        var admin = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email = AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
            FirstName = "System",
            LastName = "Admin",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded admin user {Email}", AdminEmail);
    }
}
