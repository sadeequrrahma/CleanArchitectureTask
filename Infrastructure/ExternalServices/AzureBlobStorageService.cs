using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Helpers;
using Microsoft.Extensions.Options;

namespace CleanArchitectureTask.Infrastructure.ExternalServices;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient? _client;
    private readonly AzureBlobStorageOptions _options;

    public AzureBlobStorageService(IOptions<AzureBlobStorageOptions> options)
    {
        _options = options.Value;
        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            _client = new BlobServiceClient(_options.ConnectionString);
    }

    public string? GetPublicUrl(string? blobPath)
    {
        if (_client is null || string.IsNullOrWhiteSpace(blobPath))
            return null;

        var container = _client.GetBlobContainerClient(_options.UserProfilesContainer);
        return container.GetBlobClient(blobPath).Uri.AbsoluteUri;
    }

    public async Task<string> UploadUserProfileImageAsync(
        Guid userId,
        Stream content,
        string contentType,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set AzureStorage:ConnectionString (use your Azure Storage account or local Azurite).");
        }

        var ext = fileExtension.StartsWith('.') ? fileExtension : "." + fileExtension;
        var blobPath = $"{userId:N}/profile{ext}";

        var container = _client.GetBlobContainerClient(_options.UserProfilesContainer);
        await container.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobPath);
        var headers = new BlobHttpHeaders { ContentType = contentType };

        await blob.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = headers },
            cancellationToken: cancellationToken);

        return blobPath;
    }

    public async Task DeleteBlobAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        if (_client is null || string.IsNullOrWhiteSpace(blobPath))
            return;

        var container = _client.GetBlobContainerClient(_options.UserProfilesContainer);
        var blob = container.GetBlobClient(blobPath);
        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }
}
