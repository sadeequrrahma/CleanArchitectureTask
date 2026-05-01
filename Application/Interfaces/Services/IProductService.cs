using CleanArchitectureTask.Application.DTOs.Product;
using CleanArchitectureTask.Common.Models;

namespace CleanArchitectureTask.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
