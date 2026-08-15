using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class SkillListingService : ISkillListingService
{
    private readonly SkillSwapDbContext _dbContext;

    public SkillListingService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SkillListing> CreateAsync(
        Guid userId,
        CreateSkillListingRequest request,
        CancellationToken cancellationToken = default)
    {
        bool userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == userId, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("User not found.");
        }

        var listing = new SkillListing
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            RadiusKm = request.RadiusKm,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.SkillListings.Add(listing);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return listing;
    }

    public async Task<List<SkillListing>> GetActiveAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.SkillListings
            .Include(x => x.User)
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SkillListing>> GetNearbyAsync(
    Guid userId,
    double radiusKm,
    CancellationToken cancellationToken = default)
    {
        var userAddress = await _dbContext.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (userAddress is null)
        {
            throw new InvalidOperationException(
                "User address has not been configured.");
        }

        var listings = await _dbContext.SkillListings
            .Include(x => x.User)
            .ThenInclude(x => x.Address)
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.UserId != userId &&
                x.User.Address != null)
            .ToListAsync(cancellationToken);

        var nearbyListings = listings
            .Where(x =>
            {
                if (x.User.Address is null)
                {
                    return false;
                }

                double distanceKm = CalculateDistanceKm(
                    userAddress.Latitude,
                    userAddress.Longitude,
                    x.User.Address.Latitude,
                    x.User.Address.Longitude);

                return distanceKm <= radiusKm;
            })
            .ToList();

        return nearbyListings;
    }

    private static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusKm = 6371.0;

        double latitudeDifference =
            DegreesToRadians(latitude2 - latitude1);

        double longitudeDifference =
            DegreesToRadians(longitude2 - longitude1);

        double a =
            Math.Sin(latitudeDifference / 2) *
            Math.Sin(latitudeDifference / 2) +
            Math.Cos(DegreesToRadians(latitude1)) *
            Math.Cos(DegreesToRadians(latitude2)) *
            Math.Sin(longitudeDifference / 2) *
            Math.Sin(longitudeDifference / 2);

        double c =
            2 * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public async Task<List<SkillMatchDto>> FindMatchesAsync(
    Guid userId,
    Guid listingId,
    CancellationToken cancellationToken = default)
    {
        var sourceListing = await _dbContext.SkillListings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == listingId &&
                     x.UserId == userId &&
                     x.IsActive,
                cancellationToken);

        if (sourceListing is null)
        {
            throw new InvalidOperationException(
                "Listing not found or does not belong to the current user.");
        }

        var userAddress = await _dbContext.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (userAddress is null)
        {
            throw new InvalidOperationException(
                "User address has not been configured.");
        }

        // Offer matches Need, Need matches Offer.
        SkillListingType matchingType =
    sourceListing.Type == SkillListingType.Offer
        ? SkillListingType.Need
        : SkillListingType.Offer;

        var listings = await _dbContext.SkillListings
            .Include(x => x.User)
            .ThenInclude(x => x.Address)
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.UserId != userId &&
                x.Type == matchingType &&
                x.Category == sourceListing.Category &&
                x.User.Address != null)
            .ToListAsync(cancellationToken);

        return listings
            .Select(x =>
            {
                double distanceKm = CalculateDistanceKm(
                    userAddress.Latitude,
                    userAddress.Longitude,
                    x.User.Address!.Latitude,
                    x.User.Address.Longitude);

                return new
                {
                    Listing = x,
                    DistanceKm = distanceKm
                };
            })
            .Where(x => x.DistanceKm <= sourceListing.RadiusKm)
            .OrderBy(x => x.DistanceKm)
            .Select(x => new SkillMatchDto
            {
                ListingId = x.Listing.Id,
                Type = (int)x.Listing.Type,
                Title = x.Listing.Title,
                Description = x.Listing.Description,
                Category = x.Listing.Category,
                DistanceKm = Math.Round(x.DistanceKm, 2),
                UserId = x.Listing.User.Id,
                FirstName = x.Listing.User.FirstName,
                LastName = x.Listing.User.LastName
            })
            .ToList();
    }
}