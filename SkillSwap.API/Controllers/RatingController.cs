using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRatingRequest request,
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
            var rating = await _ratingService.CreateAsync(
                parsedUserId,
                request.MatchId,
                request.Score,
                request.Review,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    rating.Id,
                    rating.MatchId,
                    rating.FromUserId,
                    rating.ToUserId,
                    rating.Score,
                    rating.Review,
                    rating.CreatedAtUtc
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

    [HttpGet("match/{matchId:guid}/mine")]
    public async Task<IActionResult> GetMyRating(
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

        var rating = await _ratingService.GetMyRatingForMatchAsync(
            parsedUserId,
            matchId,
            cancellationToken);

        if (rating == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            rating.Id,
            rating.MatchId,
            rating.FromUserId,
            rating.ToUserId,
            rating.Score,
            rating.Review,
            rating.CreatedAtUtc
        });
    }
}

public class CreateRatingRequest
{
    public Guid MatchId { get; set; }

    public int Score { get; set; }

    public string? Review { get; set; }
}