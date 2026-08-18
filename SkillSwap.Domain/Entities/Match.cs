using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }

    public Guid OfferId { get; set; }

    public Guid NeedId { get; set; }

    // User who initiated the match.
    public Guid InitiatedByUserId { get; set; }

    public MatchStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public SkillListing Offer { get; set; } = null!;

    public SkillListing Need { get; set; } = null!;

    public User InitiatedByUser { get; set; } = null!;
    // User who has marked the exchange as completed.
    // The second participant completes the exchange finally.
    public Guid? CompletedByUserId { get; set; }
}