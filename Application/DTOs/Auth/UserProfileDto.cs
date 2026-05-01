namespace CleanArchitectureTask.Application.DTOs.Auth;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    /// <summary>Public HTTPS URL of the profile image in Azure Blob Storage, when configured.</summary>
    public string? ProfileImageUrl { get; set; }
}
