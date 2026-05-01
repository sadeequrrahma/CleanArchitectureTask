using AutoMapper;
using CleanArchitectureTask.Application.DTOs.Product;
using CleanArchitectureTask.Application.Interfaces.Repositories;
using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Common.Models;
using CleanArchitectureTask.Domain.Entities;

namespace CleanArchitectureTask.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Product>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        var created = await _productRepository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ProductDto>(created);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var (items, totalCount) = await _productRepository.GetPagedAsync(page, pageSize, cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IReadOnlyList<ProductDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _productRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapper.Map<ProductDto>(entity);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<ProductDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        await _productRepository.SoftDeleteAsync(entity, cancellationToken);
        return true;
    }
}
