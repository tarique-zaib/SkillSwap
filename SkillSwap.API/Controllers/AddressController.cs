using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Application.Interfaces;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpPost]
    public async Task<IActionResult> SaveAddress(
        [FromBody] SaveAddressRequest request,
        CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            return Unauthorized(new
            {
                message = "Invalid authentication token."
            });
        }

        try
        {
            var address = await _addressService.SaveAddressAsync(
                parsedUserId,
                request,
                cancellationToken);

            return Ok(new
            {
                address.Id,
                address.City,
                address.State,
                address.PostalCode,
                address.IsVerified
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}