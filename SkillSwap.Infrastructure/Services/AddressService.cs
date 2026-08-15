using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly SkillSwapDbContext _dbContext;

    public AddressService(SkillSwapDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Address> SaveAddressAsync(
        Guid userId,
        SaveAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == userId, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("User not found.");
        }

        var address = await _dbContext.Addresses
            .SingleOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (address is null)
        {
            address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };

            _dbContext.Addresses.Add(address);
        }

        address.AddressLine1 = request.AddressLine1.Trim();
        address.AddressLine2 = request.AddressLine2?.Trim();
        address.City = request.City.Trim();
        address.State = request.State.Trim();
        address.PostalCode = request.PostalCode.Trim();
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;

        // Address verification will be implemented later.
        address.IsVerified = false;
        address.VerifiedAtUtc = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return address;
    }
}