using CleanArchitectureTask.Application.DTOs.Auth;

namespace CleanArchitectureTask.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> UpdateProfileImageAsync(
        Guid userId,
        Stream content,
        string fileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default);
}
