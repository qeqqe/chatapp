using ChatApp.Application.Common.Models;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.Features.Messages;

public record CreateMessageCommand
{
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; } = MessageType.Text;
    public Guid ChatId { get; init; }
    public Guid SenderId { get; init; }
}

public record CreateMessageResult
{
    public MessageDto Message { get; init; } = null!;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}