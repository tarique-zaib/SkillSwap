using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReciprocityController : ControllerBase
{
    private readonly IReciprocityService _reciprocityService;

    public ReciprocityController(
        IReciprocityService reciprocityService)
    {
        _reciprocityService = reciprocityService;
    }

    // GET /api/Reciprocity/summary
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

        var summary = await _reciprocityService.GetSummaryAsync(
            userId,
            cancellationToken);

        return Ok(summary);
    }
}