using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class MatchService : IMatchService
{
    private readonly SkillSwapDbContext _dbContext;

    public MatchService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Match> CreateAsync(
        Guid userId,
        Guid offerId,
        Guid needId,
        CancellationToken cancellationToken = default)
    {
        var offer = await _dbContext.SkillListings
            .FirstOrDefaultAsync(
                x => x.Id == offerId &&
                     x.UserId == userId &&
                     x.Type == SkillListingType.Offer &&
                     x.IsActive,
                cancellationToken);

        if (offer is null)
        {
            throw new InvalidOperationException(
                "Offer not found or does not belong to the current user.");
        }

        var need = await _dbContext.SkillListings
            .FirstOrDefaultAsync(
                x => x.Id == needId &&
                     x.Type == SkillListingType.Need &&
                     x.IsActive,
                cancellationToken);

        if (need is null)
        {
            throw new InvalidOperationException(
                "Need not found.");
        }

        if (need.UserId == userId)
        {
            throw new InvalidOperationException(
                "You cannot create a match with your own listing.");
        }

        if (!string.Equals(
                offer.Category,
                need.Category,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Offer and need must belong to the same category.");
        }

        bool alreadyExists = await _dbContext.Matches
            .AnyAsync(
                x => x.OfferId == offerId &&
                     x.NeedId == needId,
                cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "This match already exists.");
        }

        var match = new Match
        {
            Id = Guid.NewGuid(),
            OfferId = offerId,
            NeedId = needId,
            Status = MatchStatus.Proposed,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Matches.Add(match);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return match;
    }

    public async Task<Match> RespondAsync(
    Guid userId,
    Guid matchId,
    MatchStatus status,
    CancellationToken cancellationToken = default)
    {
        if (status != MatchStatus.Accepted &&
            status != MatchStatus.Declined)
        {
            throw new InvalidOperationException(
                "A match can only be accepted or declined.");
        }

        var match = await _dbContext.Matches
            .Include(x => x.Need)
            .FirstOrDefaultAsync(
                x => x.Id == matchId,
                cancellationToken);

        if (match is null)
        {
            throw new InvalidOperationException(
                "Match not found.");
        }

        // The owner of the Need responds to the proposed match.
        if (match.Need.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "Only the need owner can respond to this match.");
        }

        if (match.Status != MatchStatus.Proposed)
        {
            throw new InvalidOperationException(
                "This match has already been responded to.");
        }

        match.Status = status;
        match.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return match;
    }

    public async Task<List<Match>> GetMyMatchesAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Matches
            .Include(x => x.Offer)
            .Include(x => x.Need)
            .Where(x =>
                x.Offer.UserId == userId ||
                x.Need.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}