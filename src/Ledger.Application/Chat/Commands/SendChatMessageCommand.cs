using Ledger.Application.Chat.DTOs;
using MediatR;

namespace Ledger.Application.Chat.Commands;

public record SendChatMessageCommand(
    string Message,
    IReadOnlyList<ChatHistoryItem>? History) : IRequest<ChatResponseDto>;
