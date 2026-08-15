using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Interfaces;

public interface IChatService
{
    Task<ConversationDto> GetOrCreateConversationAsync(
        Guid userId,
        Guid matchId,
        CancellationToken cancellationToken = default);

    Task<MessageDto> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        string content,
        CancellationToken cancellationToken = default);

    Task<List<MessageDto>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default);
}