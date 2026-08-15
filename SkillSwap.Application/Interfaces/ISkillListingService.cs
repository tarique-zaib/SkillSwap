using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface ISkillListingService
{
    Task<SkillListing> CreateAsync(
        Guid userId,
        CreateSkillListingRequest request,
        CancellationToken cancellationToken = default);
    Task<List<SkillListing>> GetActiveAsync(
    CancellationToken cancellationToken = default);

    Task<List<SkillListing>> GetNearbyAsync(
    Guid userId,
    double radiusKm,
    CancellationToken cancellationToken = default);

    Task<List<SkillMatchDto>> FindMatchesAsync(
    Guid userId,
    Guid listingId,
    CancellationToken cancellationToken = default);
}