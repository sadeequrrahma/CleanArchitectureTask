using CleanArchitectureTask.Application.Interfaces.Repositories;
using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Helpers;
using CleanArchitectureTask.Infrastructure.Data;
using CleanArchitectureTask.Infrastructure.ExternalServices;
using CleanArchitectureTask.Infrastructure.Jwt;
using CleanArchitectureTask.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureTask.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }
}
