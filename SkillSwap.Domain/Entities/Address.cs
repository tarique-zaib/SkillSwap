namespace SkillSwap.Domain.Entities;

public class Address
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public User User { get; set; } = null!;
}