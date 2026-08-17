using SkillSwap.Application.Authentication.DTOs;

namespace SkillSwap.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<CurrentUserProfileDto?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}