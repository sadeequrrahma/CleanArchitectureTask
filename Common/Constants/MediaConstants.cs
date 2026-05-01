namespace CleanArchitectureTask.Common.Constants;

public static class MediaConstants
{
    public const long MaxProfileImageBytes = 5 * 1024 * 1024;

    public static readonly HashSet<string> AllowedProfileImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    ];

    public static readonly HashSet<string> AllowedProfileImageContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif"
    ];
}
