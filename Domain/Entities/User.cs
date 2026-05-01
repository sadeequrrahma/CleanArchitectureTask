using CleanArchitectureTask.Domain.Enums;

namespace CleanArchitectureTask.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>
    /// Relative path within the Azure Blob container (e.g. "guid/profile.jpg"). Null if no image uploaded.
    /// </summary>
    public string? ProfileImageBlobPath { get; set; }
}
