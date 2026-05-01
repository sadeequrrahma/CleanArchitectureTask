using CleanArchitectureTask.Application.DTOs.Auth;

namespace CleanArchitectureTask.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
