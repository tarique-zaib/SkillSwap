namespace SkillSwap.Application.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderUserId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}