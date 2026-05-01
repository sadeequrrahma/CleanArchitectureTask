using CleanArchitectureTask.Domain.Enums;

namespace CleanArchitectureTask.Application.Interfaces.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(Guid userId, string email, UserRole role);
}
