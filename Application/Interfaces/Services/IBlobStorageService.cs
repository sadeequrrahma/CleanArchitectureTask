namespace CleanArchitectureTask.Application.Interfaces.Services;

public interface IBlobStorageService
{
    /// <summary>Returns the public URL for a blob path, or null if not configured or no path.</summary>
    string? GetPublicUrl(string? blobPath);

    Task<string> UploadUserProfileImageAsync(
        Guid userId,
        Stream content,
        string contentType,
        string fileExtension,
        CancellationToken cancellationToken = default);

    Task DeleteBlobAsync(string blobPath, CancellationToken cancellationToken = default);
}
