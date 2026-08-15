namespace SkillSwap.Application.DTOs;

public class ConversationDto
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<MessageDto> Messages { get; set; }
        = new();
}