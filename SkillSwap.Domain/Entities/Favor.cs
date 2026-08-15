namespace SkillSwap.Domain.Entities;

public class Favor
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public DateTime? ScheduledTimeUtc { get; set; }

    public DateTime? CompletedTimeUtc { get; set; }

    public string? Notes { get; set; }

    public Match Match { get; set; } = null!;
}