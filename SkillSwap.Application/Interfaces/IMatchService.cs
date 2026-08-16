using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Interfaces;

public interface IMatchService
{
    Task<Match> CreateAsync(
        Guid userId,
        Guid offerId,
        Guid needId,
        CancellationToken cancellationToken = default);

    Task<Match> RespondAsync(
        Guid userId,
        Guid matchId,
        MatchStatus status,
        CancellationToken cancellationToken = default);

    Task<Match> CompleteAsync(
        Guid userId,
        Guid matchId,
        CancellationToken cancellationToken = default);

    Task<List<Match>> GetMyMatchesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}