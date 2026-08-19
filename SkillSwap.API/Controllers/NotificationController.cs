using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test(
    CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        var notification = await _notificationService.CreateAsync(
            userId,
            "Test",
            "Test Notification",
            "Notification system is working.",
            null,
            cancellationToken);

        return Ok(notification);
    }

    // GET /api/Notification/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        var notifications =
            await _notificationService.GetMyNotificationsAsync(
                userId,
                cancellationToken);

        return Ok(notifications.Select(ToResponse));
    }

    // PUT /api/Notification/{notificationId}/read
    [HttpPut("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            await _notificationService.MarkAsReadAsync(
                userId,
                notificationId,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // PUT /api/Notification/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(
            userId,
            cancellationToken);

        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(claim, out userId);
    }

    private static object ToResponse(
        Notification notification)
    {
        return new
        {
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.RelatedId,
            notification.IsRead,
            notification.CreatedAtUtc,
            notification.ReadAtUtc
        };
    }
}