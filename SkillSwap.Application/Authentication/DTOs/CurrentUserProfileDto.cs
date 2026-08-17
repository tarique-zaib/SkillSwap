namespace SkillSwap.Application.Authentication.DTOs;

public class CurrentUserProfileDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsAddressVerified { get; set; }

    public AddressProfileDto? Address { get; set; }
}

public class AddressProfileDto
{
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public bool IsVerified { get; set; }
}