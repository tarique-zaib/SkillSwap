using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Interfaces;

public interface IReciprocityService
{
    Task<ReciprocitySummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}