using CleanArchitectureTask.Domain.Entities;

namespace CleanArchitectureTask.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
