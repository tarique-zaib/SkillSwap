using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface IVouchService
{
    Task<Vouch> CreateAsync(
        Guid fromUserId,
        Guid favorId,
        string skillCategory,
        string? storyText,
        CancellationToken cancellationToken = default);

    Task<List<Vouch>> GetReceivedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<Vouch>> GetGivenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}