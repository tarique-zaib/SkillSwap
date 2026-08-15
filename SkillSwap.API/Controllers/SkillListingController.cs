using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Application.Interfaces;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SkillListingController : ControllerBase
{
    private readonly ISkillListingService _skillListingService;

    public SkillListingController(
        ISkillListingService skillListingService)
    {
        _skillListingService = skillListingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSkillListingRequest request,
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
            var listing = await _skillListingService.CreateAsync(
                parsedUserId,
                request,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    listing.Id,
                    listing.Type,
                    listing.Title,
                    listing.Description,
                    listing.Category,
                    listing.RadiusKm,
                    listing.IsActive,
                    listing.CreatedAtUtc
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

    [HttpGet]
    public async Task<IActionResult> GetActive(
    CancellationToken cancellationToken)
    {
        var listings = await _skillListingService.GetActiveAsync(
            cancellationToken);

        return Ok(listings.Select(x => new
        {
            x.Id,
            x.Type,
            x.Title,
            x.Description,
            x.Category,
            x.RadiusKm,
            x.IsActive,
            x.CreatedAtUtc,

            User = new
            {
                x.User.Id,
                x.User.FirstName,
                x.User.LastName
            }
        }));
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
    [FromQuery] double radiusKm = 2,
    CancellationToken cancellationToken = default)
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

        if (radiusKm <= 0 || radiusKm > 50)
        {
            return BadRequest(new
            {
                message = "Radius must be between 0 and 50 km."
            });
        }

        try
        {
            var listings = await _skillListingService.GetNearbyAsync(
                parsedUserId,
                radiusKm,
                cancellationToken);

            return Ok(listings.Select(x => new
            {
                x.Id,
                x.Type,
                x.Title,
                x.Description,
                x.Category,
                x.RadiusKm,
                x.IsActive,
                x.CreatedAtUtc,

                User = new
                {
                    x.User.Id,
                    x.User.FirstName,
                    x.User.LastName
                }
            }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("{listingId:guid}/matches")]
    public async Task<IActionResult> FindMatches(
    Guid listingId,
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
            var matches = await _skillListingService.FindMatchesAsync(
                parsedUserId,
                listingId,
                cancellationToken);

            return Ok(matches);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}