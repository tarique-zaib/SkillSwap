using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly SkillSwapDbContext _dbContext;

    public ChatService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConversationDto> GetOrCreateConversationAsync(
        Guid userId,
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        var match = await _dbContext.Matches
            .Include(x => x.Offer)
            .Include(x => x.Need)
            .FirstOrDefaultAsync(
                x => x.Id == matchId,
                cancellationToken);

        if (match is null)
        {
            throw new InvalidOperationException(
                "Match not found.");
        }

        EnsureParticipant(match, userId);

        if (match.Status != MatchStatus.Accepted &&
            match.Status != MatchStatus.Completed)
        {
            throw new InvalidOperationException(
                "Chat is available only after a match is accepted.");
        }

        var conversation = await _dbContext.Conversations
            .Include(x => x.Messages)
            .ThenInclude(x => x.SenderUser)
            .FirstOrDefaultAsync(
                x => x.MatchId == matchId,
                cancellationToken);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Conversations.Add(conversation);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return ToConversationDto(conversation);
    }

    public async Task<MessageDto> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "Message content is required.");
        }

        content = content.Trim();

        if (content.Length > 5000)
        {
            throw new InvalidOperationException(
                "Message cannot exceed 5000 characters.");
        }

        var conversation = await _dbContext.Conversations
            .Include(x => x.Match)
            .ThenInclude(x => x.Offer)
            .Include(x => x.Match)
            .ThenInclude(x => x.Need)
            .FirstOrDefaultAsync(
                x => x.Id == conversationId,
                cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException(
                "Conversation not found.");
        }

        EnsureParticipant(conversation.Match, userId);

        if (conversation.Match.Status != MatchStatus.Accepted &&
            conversation.Match.Status != MatchStatus.Completed)
        {
            throw new InvalidOperationException(
                "Messages can only be sent for an active match.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = userId,
            Content = content,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var sender = await _dbContext.Users
            .AsNoTracking()
            .FirstAsync(
                x => x.Id == userId,
                cancellationToken);

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            SenderName =
                $"{sender.FirstName} {sender.LastName}".Trim(),
            Content = message.Content,
            CreatedAtUtc = message.CreatedAtUtc
        };
    }

    public async Task<List<MessageDto>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _dbContext.Conversations
            .Include(x => x.Match)
            .ThenInclude(x => x.Offer)
            .Include(x => x.Match)
            .ThenInclude(x => x.Need)
            .FirstOrDefaultAsync(
                x => x.Id == conversationId,
                cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException(
                "Conversation not found.");
        }

        EnsureParticipant(conversation.Match, userId);

        return await _dbContext.Messages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new MessageDto
            {
                Id = x.Id,
                ConversationId = x.ConversationId,
                SenderUserId = x.SenderUserId,
                SenderName =
                    $"{x.SenderUser.FirstName} {x.SenderUser.LastName}".Trim(),
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    private static void EnsureParticipant(
        Match match,
        Guid userId)
    {
        bool isParticipant =
            match.Offer.UserId == userId ||
            match.Need.UserId == userId;

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException(
                "You are not a participant in this match.");
        }
    }

    private static ConversationDto ToConversationDto(
        Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            MatchId = conversation.MatchId,
            CreatedAtUtc = conversation.CreatedAtUtc,
            Messages = conversation.Messages
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new MessageDto
                {
                    Id = x.Id,
                    ConversationId = x.ConversationId,
                    SenderUserId = x.SenderUserId,
                    SenderName =
                        $"{x.SenderUser.FirstName} {x.SenderUser.LastName}".Trim(),
                    Content = x.Content,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToList()
        };
    }
}