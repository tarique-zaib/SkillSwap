using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly SkillSwapDbContext _dbContext;

    public RatingService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Rating?> GetMyRatingForMatchAsync(
    Guid userId,
    Guid matchId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Ratings
            .FirstOrDefaultAsync(
                x =>
                    x.MatchId == matchId &&
                    x.FromUserId == userId,
                cancellationToken);
    }

    public async Task<Rating> CreateAsync(
        Guid userId,
        Guid matchId,
        int score,
        string? review,
        CancellationToken cancellationToken = default)
    {
        if (score < 1 || score > 5)
        {
            throw new InvalidOperationException(
                "Rating score must be between 1 and 5.");
        }

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

        if (match.Status != MatchStatus.Completed)
        {
            throw new InvalidOperationException(
                "Only completed matches can be rated.");
        }

        Guid toUserId;

        if (match.Offer.UserId == userId)
        {
            toUserId = match.Need.UserId;
        }
        else if (match.Need.UserId == userId)
        {
            toUserId = match.Offer.UserId;
        }
        else
        {
            throw new UnauthorizedAccessException(
                "Only participants of this match can submit a rating.");
        }

        bool alreadyRated = await _dbContext.Ratings
            .AnyAsync(
                x => x.MatchId == matchId &&
                     x.FromUserId == userId,
                cancellationToken);

        if (alreadyRated)
        {
            throw new InvalidOperationException(
                "You have already rated this exchange.");
        }

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            FromUserId = userId,
            ToUserId = toUserId,
            Score = score,
            Review = string.IsNullOrWhiteSpace(review)
                ? null
                : review.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Ratings.Add(rating);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return rating;
    }
}