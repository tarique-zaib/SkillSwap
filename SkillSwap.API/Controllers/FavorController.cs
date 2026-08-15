using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FavorController : ControllerBase
{
    private readonly IFavorService _favorService;

    public FavorController(IFavorService favorService)
    {
        _favorService = favorService;
    }

    // POST /api/Favor
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromQuery] Guid matchId,
        [FromQuery] DateTime? scheduledTimeUtc,
        [FromQuery] string? notes,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var favor = await _favorService.CreateAsync(
                userId,
                matchId,
                scheduledTimeUtc,
                notes,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                ToResponse(favor));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/Favor/{favorId}/schedule
    [HttpPut("{favorId:guid}/schedule")]
    public async Task<IActionResult> Schedule(
        Guid favorId,
        [FromQuery] DateTime scheduledTimeUtc,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var favor = await _favorService.ScheduleAsync(
                userId,
                favorId,
                scheduledTimeUtc,
                cancellationToken);

            return Ok(ToResponse(favor));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/Favor/{favorId}/complete
    [HttpPut("{favorId:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid favorId,
        [FromQuery] string? notes,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var favor = await _favorService.CompleteAsync(
                userId,
                favorId,
                notes,
                cancellationToken);

            return Ok(ToResponse(favor));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/Favor/{favorId}
    [HttpGet("{favorId:guid}")]
    public async Task<IActionResult> GetById(
        Guid favorId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var favor = await _favorService.GetByIdAsync(
                userId,
                favorId,
                cancellationToken);

            if (favor is null)
                return NotFound();

            return Ok(ToResponse(favor));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // GET /api/Favor/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyFavors(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        var favors = await _favorService.GetMyFavorsAsync(
            userId,
            cancellationToken);

        return Ok(favors.Select(ToResponse));
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(claim, out userId);
    }

    private static object ToResponse(Favor favor)
    {
        return new
        {
            favor.Id,
            favor.MatchId,
            favor.ScheduledTimeUtc,
            favor.CompletedTimeUtc,
            favor.Notes,
            MatchStatus = favor.Match?.Status
        };
    }
}