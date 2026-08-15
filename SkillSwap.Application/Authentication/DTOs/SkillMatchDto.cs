namespace SkillSwap.Application.Authentication.DTOs;

public class SkillMatchDto
{
    public Guid ListingId { get; set; }

    public int Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public double DistanceKm { get; set; }

    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}