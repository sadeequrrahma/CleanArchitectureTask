using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Application.MappingProfiles;
using CleanArchitectureTask.Application.Services;
using CleanArchitectureTask.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureTask.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddValidatorsFromAssembly(typeof(RegisterRequestValidator).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserProfileService, UserProfileService>();

        return services;
    }
}
