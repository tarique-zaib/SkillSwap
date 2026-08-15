namespace SkillSwap.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Match Match { get; set; } = null!;

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();
}