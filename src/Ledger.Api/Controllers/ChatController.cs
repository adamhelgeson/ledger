using Ledger.Application.Chat.Commands;
using Ledger.Application.Chat.DTOs;
using Ledger.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Ask Heimdall — the all-seeing financial advisor.
    /// Currently returns a rule-based placeholder; will integrate with Claude API in a future session.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ChatResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SendChatMessageCommand(request.Message, request.History), ct);
        return Ok(ApiResponse<ChatResponseDto>.Ok(result));
    }
}
