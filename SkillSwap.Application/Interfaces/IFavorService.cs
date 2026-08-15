using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface IFavorService
{
    Task<Favor> CreateAsync(
        Guid userId,
        Guid matchId,
        DateTime? scheduledTimeUtc,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<Favor> ScheduleAsync(
        Guid userId,
        Guid favorId,
        DateTime scheduledTimeUtc,
        CancellationToken cancellationToken = default);

    Task<Favor> CompleteAsync(
        Guid userId,
        Guid favorId,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<Favor?> GetByIdAsync(
        Guid userId,
        Guid favorId,
        CancellationToken cancellationToken = default);

    Task<List<Favor>> GetMyFavorsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}