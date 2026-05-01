using CleanArchitectureTask.Application.Interfaces.Repositories;
using CleanArchitectureTask.Domain.Entities;
using CleanArchitectureTask.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTask.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await DbSet.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }
}
