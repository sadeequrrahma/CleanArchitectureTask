using CleanArchitectureTask.Application.DTOs.Auth;
using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Common.Constants;
using CleanArchitectureTask.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserProfileService _userProfileService;

    public UsersController(IAuthService authService, IUserProfileService userProfileService)
    {
        _authService = authService;
        _userProfileService = userProfileService;
    }

    /// <summary>Use <c>userId</c> from login/register (<c>data.user.id</c>).</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _authService.GetProfileAsync(userId, cancellationToken);
        if (profile is null)
            return NotFound(ApiResponse<UserProfileDto>.Fail("User not found."));

        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    /// <summary>Multipart field name: <c>file</c>. Requires <c>userId</c> query.</summary>
    [HttpPut("profile-image")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MediaConstants.MaxProfileImageBytes)]
    [RequestSizeLimit(MediaConstants.MaxProfileImageBytes)]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadProfileImage(
        [FromQuery] Guid userId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<UserProfileDto>.Fail("Image file is required."));

        await using var stream = file.OpenReadStream();
        var updated = await _userProfileService.UpdateProfileImageAsync(
            userId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        if (updated is null)
            return NotFound(ApiResponse<UserProfileDto>.Fail("User not found."));

        return Ok(ApiResponse<UserProfileDto>.Ok(updated, "Profile image updated."));
    }
}
