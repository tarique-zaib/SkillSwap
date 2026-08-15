using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class VouchService : IVouchService
{
    private readonly SkillSwapDbContext _dbContext;

    public VouchService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Vouch> CreateAsync(
        Guid fromUserId,
        Guid favorId,
        string skillCategory,
        string? storyText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(skillCategory))
        {
            throw new InvalidOperationException(
                "Skill category is required.");
        }

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

        if (!favor.CompletedTimeUtc.HasValue ||
            favor.Match.Status != Domain.Enums.MatchStatus.Completed)
        {
            throw new InvalidOperationException(
                "A vouch can only be created for a completed favor.");
        }

        Guid toUserId;

        if (favor.Match.Offer.UserId == fromUserId)
        {
            toUserId = favor.Match.Need.UserId;
        }
        else if (favor.Match.Need.UserId == fromUserId)
        {
            toUserId = favor.Match.Offer.UserId;
        }
        else
        {
            throw new UnauthorizedAccessException(
                "You were not a participant in this favor.");
        }

        bool alreadyVouched = await _dbContext.Vouches
            .AnyAsync(
                x => x.FavorId == favorId &&
                     x.FromUserId == fromUserId &&
                     x.ToUserId == toUserId,
                cancellationToken);

        if (alreadyVouched)
        {
            throw new InvalidOperationException(
                "You have already vouched for this user for this favor.");
        }

        var vouch = new Vouch
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            FavorId = favorId,
            SkillCategory = skillCategory.Trim(),
            StoryText = string.IsNullOrWhiteSpace(storyText)
                ? null
                : storyText.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Vouches.Add(vouch);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return vouch;
    }

    public async Task<List<Vouch>> GetReceivedAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vouches
            .AsNoTracking()
            .Include(x => x.FromUser)
            .Include(x => x.Favor)
            .Where(x => x.ToUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Vouch>> GetGivenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vouches
            .AsNoTracking()
            .Include(x => x.ToUser)
            .Include(x => x.Favor)
            .Where(x => x.FromUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}