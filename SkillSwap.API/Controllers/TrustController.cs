using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TrustController : ControllerBase
{
    private readonly ITrustService _trustService;

    public TrustController(ITrustService trustService)
    {
        _trustService = trustService;
    }

    // GET /api/Trust/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken)
    {
        string? claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(claim, out Guid userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authentication token."
            });
        }

        var summary = await _trustService.GetSummaryAsync(
            userId,
            cancellationToken);

        return Ok(summary);
    }
}