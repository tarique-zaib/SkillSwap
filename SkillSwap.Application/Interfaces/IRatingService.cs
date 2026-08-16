using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface IRatingService
{
    Task<Rating> CreateAsync(
        Guid userId,
        Guid matchId,
        int score,
        string? review,
        CancellationToken cancellationToken = default);

    Task<Rating> GetMyRatingForMatchAsync(
        Guid userId,
        Guid matchId,
        CancellationToken cancellationToken = default);
}