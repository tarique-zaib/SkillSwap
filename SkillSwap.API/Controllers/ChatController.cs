using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using SkillSwap.API.Hubs;

namespace SkillSwap.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;


    public ChatController(
    IChatService chatService,
    IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    // GET /api/Chat/match/{matchId}
    [HttpGet("match/{matchId:guid}")]
    public async Task<IActionResult> GetOrCreateConversation(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var conversation =
                await _chatService.GetOrCreateConversationAsync(
                    userId,
                    matchId,
                    cancellationToken);

            return Ok(conversation);
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

    // POST /api/Chat/{conversationId}/messages
    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var message = await _chatService.SendMessageAsync(
            userId,
            conversationId,
            request.Content,
            cancellationToken);

            await _hubContext.Clients
                .Group($"conversation:{conversationId}")
                .SendAsync(
                    "NewMessage",
                    message,
                    cancellationToken);

                return StatusCode(
                StatusCodes.Status201Created,
                message);
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

    // GET /api/Chat/{conversationId}/messages
    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
            return Unauthorized();

        try
        {
            var messages = await _chatService.GetMessagesAsync(
                userId,
                conversationId,
                cancellationToken);

            return Ok(messages);
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

    private bool TryGetUserId(out Guid userId)
    {
        string? claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(claim, out userId);
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}