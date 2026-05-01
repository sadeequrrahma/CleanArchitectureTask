using AutoMapper;
using CleanArchitectureTask.Application.DTOs.Auth;
using CleanArchitectureTask.Application.Interfaces.Repositories;
using CleanArchitectureTask.Application.Interfaces.Services;
using CleanArchitectureTask.Domain.Entities;
using CleanArchitectureTask.Domain.Enums;

namespace CleanArchitectureTask.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IBlobStorageService _blobStorage;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IBlobStorageService blobStorage,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _blobStorage = blobStorage;
        _mapper = mapper;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return BuildAuthResponse(user);
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return null;

        return MapUserToProfileDto(user);
    }

    private AuthResponse BuildAuthResponse(User user)
        => new() { User = MapUserToProfileDto(user) };

    private UserProfileDto MapUserToProfileDto(User user)
    {
        var dto = _mapper.Map<UserProfileDto>(user);
        dto.ProfileImageUrl = _blobStorage.GetPublicUrl(user.ProfileImageBlobPath);
        return dto;
    }
}
