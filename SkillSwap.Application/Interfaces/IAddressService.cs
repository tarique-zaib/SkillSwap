using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface IAddressService
{
    Task<Address> SaveAddressAsync(
        Guid userId,
        SaveAddressRequest request,
        CancellationToken cancellationToken = default);
}