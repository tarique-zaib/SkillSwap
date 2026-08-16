namespace SkillSwap.Domain.Entities;

public class Rating
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public int Score { get; set; }

    public string? Review { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Match Match { get; set; } = null!;

    public User FromUser { get; set; } = null!;

    public User ToUser { get; set; } = null!;
}