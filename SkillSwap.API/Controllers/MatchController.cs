using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Enums;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromQuery] Guid offerId,
        [FromQuery] Guid needId,
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
            var match = await _matchService.CreateAsync(
                parsedUserId,
                offerId,
                needId,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    match.Id,
                    match.OfferId,
                    match.NeedId,
                    match.Status,
                    match.CreatedAtUtc
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{matchId:guid}/respond")]
    public async Task<IActionResult> Respond(
    Guid matchId,
    [FromQuery] MatchStatus status,
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
            var match = await _matchService.RespondAsync(
                parsedUserId,
                matchId,
                status,
                cancellationToken);

            return Ok(new
            {
                match.Id,
                match.OfferId,
                match.NeedId,
                match.Status,
                match.UpdatedAtUtc
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{matchId:guid}/complete")]
    public async Task<IActionResult> Complete(
    Guid matchId,
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
            var match = await _matchService.CompleteAsync(
                parsedUserId,
                matchId,
                cancellationToken);

            return Ok(new
            {
                match.Id,
                match.OfferId,
                match.NeedId,
                match.Status,
                match.UpdatedAtUtc
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyMatches(
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

        var matches = await _matchService.GetMyMatchesAsync(
            parsedUserId,
            cancellationToken);

        return Ok(matches.Select(match => new
        {
            match.Id,
            match.OfferId,
            match.NeedId,
            match.Status,
            match.CreatedAtUtc,
            match.UpdatedAtUtc,
            match.InitiatedByUserId,
            Offer = match.Offer == null
                ? null
                : new
                {
                    match.Offer.Id,
                    match.Offer.Title,
                    match.Offer.Description,
                    match.Offer.Category,
                    match.Offer.UserId
                },
            Need = match.Need == null
                ? null
                : new
                {
                    match.Need.Id,
                    match.Need.Title,
                    match.Need.Description,
                    match.Need.Category,
                    match.Need.UserId
                }
        }));
    }
}