namespace SkillSwap.Application.Authentication.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}