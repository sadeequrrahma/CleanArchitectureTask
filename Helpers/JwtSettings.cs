namespace CleanArchitectureTask.Helpers;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Symmetric key for HS256 (at least 32 characters recommended).</summary>
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
