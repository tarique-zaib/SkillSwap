using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Interfaces;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class ReciprocityService : IReciprocityService
{
    private readonly SkillSwapDbContext _dbContext;

    public ReciprocityService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReciprocitySummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var completedFavors = await _dbContext.Favors
            .AsNoTracking()
            .Include(x => x.Match)
            .ThenInclude(x => x.Offer)
            .Include(x => x.Match)
            .ThenInclude(x => x.Need)
            .Where(x => x.CompletedTimeUtc.HasValue)
            .ToListAsync(cancellationToken);

        int givenCount = 0;
        int receivedCount = 0;

        foreach (var favor in completedFavors)
        {
            var offerUserId = favor.Match.Offer.UserId;
            var needUserId = favor.Match.Need.UserId;

            if (offerUserId == userId)
            {
                givenCount++;
            }

            if (needUserId == userId)
            {
                receivedCount++;
            }
        }

        return new ReciprocitySummaryDto
        {
            GivenCount = givenCount,
            ReceivedCount = receivedCount,
            Balance = givenCount - receivedCount,
            CompletedFavorCount = givenCount + receivedCount
        };
    }
}