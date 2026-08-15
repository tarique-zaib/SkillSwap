using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Interfaces;

public interface ITrustService
{
    Task<TrustSummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}