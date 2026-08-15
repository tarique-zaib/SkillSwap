using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VouchController : ControllerBase
{
    private readonly IVouchService _vouchService;

    public VouchController(IVouchService vouchService)
    {
        _vouchService = vouchService;
    }

    // POST /api/Vouch
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromQuery] Guid favorId,
        [FromQuery] string skillCategory,
        [FromQuery] string? storyText,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var vouch = await _vouchService.CreateAsync(
                userId,
                favorId,
                skillCategory,
                storyText,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                ToResponse(vouch));
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

    // GET /api/Vouch/received
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        var vouches = await _vouchService.GetReceivedAsync(
            userId,
            cancellationToken);

        return Ok(vouches.Select(ToResponse));
    }

    // GET /api/Vouch/given
    [HttpGet("given")]
    public async Task<IActionResult> GetGiven(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        var vouches = await _vouchService.GetGivenAsync(
            userId,
            cancellationToken);

        return Ok(vouches.Select(ToResponse));
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(claim, out userId);
    }

    private static object ToResponse(Domain.Entities.Vouch vouch)
    {
        return new
        {
            vouch.Id,
            vouch.FromUserId,
            vouch.ToUserId,
            vouch.FavorId,
            vouch.SkillCategory,
            vouch.StoryText,
            vouch.CreatedAtUtc
        };
    }
}