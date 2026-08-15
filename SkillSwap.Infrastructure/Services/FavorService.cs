using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class FavorService : IFavorService
{
    private readonly SkillSwapDbContext _dbContext;

    public FavorService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Favor> CreateAsync(
        Guid userId,
        Guid matchId,
        DateTime? scheduledTimeUtc,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var match = await GetParticipantMatchAsync(
            userId,
            matchId,
            cancellationToken);

        if (match.Status != MatchStatus.Accepted)
        {
            throw new InvalidOperationException(
                "A favor can only be created from an accepted match.");
        }

        bool exists = await _dbContext.Favors
            .AnyAsync(x => x.MatchId == matchId, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "A favor already exists for this match.");
        }

        var favor = new Favor
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            ScheduledTimeUtc = scheduledTimeUtc,
            Notes = CleanNotes(notes)
        };

        _dbContext.Favors.Add(favor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return favor;
    }

    public async Task<Favor> ScheduleAsync(
        Guid userId,
        Guid favorId,
        DateTime scheduledTimeUtc,
        CancellationToken cancellationToken = default)
    {
        var favor = await GetFavorAsync(
            userId,
            favorId,
            cancellationToken);

        if (favor.CompletedTimeUtc.HasValue)
        {
            throw new InvalidOperationException(
                "A completed favor cannot be rescheduled.");
        }

        if (scheduledTimeUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Scheduled time must be in the future.");
        }

        favor.ScheduledTimeUtc = scheduledTimeUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return favor;
    }

    public async Task<Favor> CompleteAsync(
        Guid userId,
        Guid favorId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var favor = await GetFavorAsync(
            userId,
            favorId,
            cancellationToken);

        if (favor.CompletedTimeUtc.HasValue)
        {
            throw new InvalidOperationException(
                "This favor has already been completed.");
        }

        favor.CompletedTimeUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            favor.Notes = notes.Trim();
        }

        favor.Match.Status = MatchStatus.Completed;
        favor.Match.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return favor;
    }

    public async Task<Favor?> GetByIdAsync(
        Guid userId,
        Guid favorId,
        CancellationToken cancellationToken = default)
    {
        var favor = await _dbContext.Favors
            .Include(x => x.Match)
            .ThenInclude(x => x.Offer)
            .Include(x => x.Match)
            .ThenInclude(x => x.Need)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == favorId,
                cancellationToken);

        if (favor is null)
        {
            return null;
        }

        bool participant =
            favor.Match.Offer.UserId == userId ||
            favor.Match.Need.UserId == userId;

        if (!participant)
        {
            throw new UnauthorizedAccessException(
                "You are not a participant in this favor.");
        }

        return favor;
    }

    public async Task<List<Favor>> GetMyFavorsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Favors
            .Include(x => x.Match)
            .ThenInclude(x => x.Offer)
            .Include(x => x.Match)
            .ThenInclude(x => x.Need)
            .Where(x =>
                x.Match.Offer.UserId == userId ||
                x.Match.Need.UserId == userId)
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private async Task<Match> GetParticipantMatchAsync(
        Guid userId,
        Guid matchId,
        CancellationToken cancellationToken)
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

        bool participant =
            match.Offer.UserId == userId ||
            match.Need.UserId == userId;

        if (!participant)
        {
            throw new UnauthorizedAccessException(
                "You are not a participant in this match.");
        }

        return match;
    }

    private async Task<Favor> GetFavorAsync(
        Guid userId,
        Guid favorId,
        CancellationToken cancellationToken)
    {
        var favor = await _dbContext.Favors
            .Include(x => x.Match)
            .ThenInclude(x => x.Offer)
            .Include(x => x.Match)
            .ThenInclude(x => x.Need)
            .FirstOrDefaultAsync(
                x => x.Id == favorId,
                cancellationToken);

        if (favor is null)
        {
            throw new InvalidOperationException(
                "Favor not found.");
        }

        bool participant =
            favor.Match.Offer.UserId == userId ||
            favor.Match.Need.UserId == userId;

        if (!participant)
        {
            throw new UnauthorizedAccessException(
                "You are not a participant in this favor.");
        }

        return favor;
    }

    private static string? CleanNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }
}