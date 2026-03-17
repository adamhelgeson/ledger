using Ledger.Application.Chat.DTOs;
using MediatR;

namespace Ledger.Application.Chat.Commands;

public record SendChatMessageCommand(
    string Message,
    IReadOnlyList<ChatHistoryItem>? History) : IRequest<ChatResponseDto>;

public sealed class SendChatMessageHandler : IRequestHandler<SendChatMessageCommand, ChatResponseDto>
{
    // Placeholder — will integrate with Claude API in a future session.
    public Task<ChatResponseDto> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var reply = request.Message.ToLowerInvariant() switch
        {
            var m when m.Contains("net worth") =>
                "I can see your net worth across all realms. Ask me to break it down by account!",
            var m when m.Contains("spend") || m.Contains("budget") =>
                "The Ragnarök Report shows your spending this month. Shall I summarize the categories?",
            var m when m.Contains("saving") =>
                "Your Vibranium Reserves are growing steadily. Keep up the discipline, mortal.",
            _ =>
                "I am Heimdall, guardian of the Bifrost and keeper of the All-Father's Treasury. " +
                "Ask me about your accounts, spending, or net worth."
        };

        return Task.FromResult(new ChatResponseDto(reply, DateTimeOffset.UtcNow));
    }
}
