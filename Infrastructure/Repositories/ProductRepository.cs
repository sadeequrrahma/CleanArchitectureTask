using CleanArchitectureTask.Application.Interfaces.Repositories;
using CleanArchitectureTask.Domain.Entities;
using CleanArchitectureTask.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTask.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
