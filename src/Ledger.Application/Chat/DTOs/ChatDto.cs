namespace Ledger.Application.Chat.DTOs;

public record ChatMessageRequest(string Message, IReadOnlyList<ChatHistoryItem>? History = null);

public record ChatHistoryItem(string Role, string Content);

public record ChatResponseDto(string Message, DateTimeOffset Timestamp);
