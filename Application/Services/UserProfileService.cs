using CleanArchitectureTask.Application.DTOs.Auth;
using CleanArchitectureTask.Application.Interfaces.Repositories;
using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Common.Constants;

namespace CleanArchitectureTask.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IBlobStorageService _blobStorage;

    public UserProfileService(IUserRepository userRepository, IBlobStorageService blobStorage)
    {
        _userRepository = userRepository;
        _blobStorage = blobStorage;
    }

    public async Task<UserProfileDto?> UpdateProfileImageAsync(
        Guid userId,
        Stream content,
        string fileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        if (contentLength > MediaConstants.MaxProfileImageBytes)
            throw new ArgumentException($"Image must be at most {MediaConstants.MaxProfileImageBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) ||
            !MediaConstants.AllowedProfileImageExtensions.Contains(extension))
            throw new ArgumentException("Allowed image types: JPG, PNG, WEBP, GIF.");

        if (!MediaConstants.AllowedProfileImageContentTypes.Contains(contentType.ToLowerInvariant()))
            throw new ArgumentException("Invalid image content type.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return null;

        if (!string.IsNullOrEmpty(user.ProfileImageBlobPath))
            await _blobStorage.DeleteBlobAsync(user.ProfileImageBlobPath, cancellationToken);

        var blobPath = await _blobStorage.UploadUserProfileImageAsync(
            userId,
            content,
            contentType,
            extension,
            cancellationToken);

        user.ProfileImageBlobPath = blobPath;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ProfileImageUrl = _blobStorage.GetPublicUrl(blobPath)
        };
    }
}
