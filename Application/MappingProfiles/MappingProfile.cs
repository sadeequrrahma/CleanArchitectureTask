using AutoMapper;
using CleanArchitectureTask.Application.DTOs.Auth;
using CleanArchitectureTask.Application.DTOs.Product;
using CleanArchitectureTask.Domain.Entities;

namespace CleanArchitectureTask.Application.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.ProfileImageUrl, o => o.Ignore());

        CreateMap<CreateProductRequest, Product>();
        CreateMap<UpdateProductRequest, Product>();
        CreateMap<Product, ProductDto>();
    }
}
