namespace CleanArchitectureTask.Helpers;

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureStorage";

    /// <summary>Azure Storage connection string (production: Storage Account; local: Azurite).</summary>
    public string ConnectionString { get; set; } = string.Empty;

    public string UserProfilesContainer { get; set; } = "user-profiles";
}
