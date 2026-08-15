using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

public class SkillListing
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public SkillListingType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public double RadiusKm { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}