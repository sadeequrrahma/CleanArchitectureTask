using CleanArchitectureTask.Domain.Entities;

namespace CleanArchitectureTask.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
