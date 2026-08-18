using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Interfaces;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class TrustService : ITrustService
{
    private readonly SkillSwapDbContext _dbContext;

    public TrustService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TrustSummaryDto> GetSummaryAsync(
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

        int givenFavorCount = 0;
        int receivedFavorCount = 0;

        foreach (var favor in completedFavors)
        {
            if (favor.Match.Offer.UserId == userId)
            {
                givenFavorCount++;
            }

            if (favor.Match.Need.UserId == userId)
            {
                receivedFavorCount++;
            }
        }

        int completedFavorCount =
            givenFavorCount + receivedFavorCount;

        int vouchCount = await _dbContext.Vouches
            .CountAsync(
                x => x.ToUserId == userId,
                cancellationToken);

        string tier = CalculateTier(
            completedFavorCount,
            vouchCount);

        return new TrustSummaryDto
        {
            Tier = tier,
            CompletedFavorCount = completedFavorCount,
            VouchCount = vouchCount,
            GivenFavorCount = givenFavorCount,
            ReceivedFavorCount = receivedFavorCount
        };
    }

    private static string CalculateTier(
    int completedFavorCount,
    int vouchCount)
    {
        if (completedFavorCount >= 5)
        {
            return "Highly Trusted";
        }

        if (completedFavorCount >= 3)
        {
            return "Established";
        }

        if (completedFavorCount >= 1)
        {
            return "Trusted";
        }

        return "Newcomer";
    }
}