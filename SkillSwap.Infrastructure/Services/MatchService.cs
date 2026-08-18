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
                     x.Type == SkillListingType.Offer &&
                     x.IsActive,
                cancellationToken);

        if (offer is null)
        {
            throw new InvalidOperationException(
                "Offer not found.");
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

        // The current user must own either the Offer or the Need.
        bool isOfferOwner = offer.UserId == userId;
        bool isNeedOwner = need.UserId == userId;

        if (!isOfferOwner && !isNeedOwner)
        {
            throw new UnauthorizedAccessException(
                "You are not a participant in this match.");
        }

        // A user cannot match their own Offer with their own Need.
        if (offer.UserId == need.UserId)
        {
            throw new InvalidOperationException(
                "You cannot create a match with your own listings.");
        }

        //if (!string.Equals(
        //        offer.Category,
        //        need.Category,
        //        StringComparison.OrdinalIgnoreCase))
        //{
        //    throw new InvalidOperationException(
        //        "Offer and need must belong to the same category.");
        //}

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

            // Remember who initiated this match.
            InitiatedByUserId = userId,

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

        // The person who DID NOT initiate the match responds.
        if (match.InitiatedByUserId == userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot respond to your own match request.");
        }

        // Make sure the responder is actually the other participant.
        bool isOfferOwner = match.Offer.UserId == userId;
        bool isNeedOwner = match.Need.UserId == userId;

        if (!isOfferOwner && !isNeedOwner)
        {
            throw new UnauthorizedAccessException(
                "Only the other participant can respond to this match.");
        }

        if (match.Status != MatchStatus.Proposed)
        {
            throw new InvalidOperationException(
                "This match has already been responded to.");
        }

        match.Status = status;
        match.UpdatedAtUtc = DateTime.UtcNow;

        // When a match is accepted, create the Favor automatically.
        // The user should not have to create a separate Favor.
        if (status == MatchStatus.Accepted)
        {
            bool favorExists = await _dbContext.Favors
                .AnyAsync(
                    x => x.MatchId == match.Id,
                    cancellationToken);

            if (!favorExists)
            {
                var favor = new Favor
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    ScheduledTimeUtc = null,
                    CompletedTimeUtc = null,
                    Notes = null
                };

                _dbContext.Favors.Add(favor);
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return match;
    }

    public async Task<Match> CompleteAsync(
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

        // Only participants in the match can complete it.
        bool isParticipant =
            match.Offer.UserId == userId ||
            match.Need.UserId == userId;

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException(
                "Only participants of this match can complete it.");
        }

        // Already completed by both participants.
        if (match.Status == MatchStatus.Completed)
        {
            throw new InvalidOperationException(
                "This exchange has already been completed.");
        }

        // Only an accepted match can be completed.
        if (match.Status != MatchStatus.Accepted)
        {
            throw new InvalidOperationException(
                "Only an accepted match can be completed.");
        }

        // First participant marks the exchange complete.
        if (match.CompletedByUserId is null)
        {
            match.CompletedByUserId = userId;
            match.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return match;
        }

        // The same participant cannot mark it complete twice.
        if (match.CompletedByUserId == userId)
        {
            throw new InvalidOperationException(
                "You have already marked this exchange as complete. Waiting for the other participant.");
        }

        // The other participant has now confirmed completion.
        var favor = await _dbContext.Favors
            .FirstOrDefaultAsync(
                x => x.MatchId == match.Id,
                cancellationToken);

        if (favor is null)
        {
            throw new InvalidOperationException(
                "Favor not found for this exchange.");
        }

        favor.CompletedTimeUtc = DateTime.UtcNow;

        match.Status = MatchStatus.Completed;
        match.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

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