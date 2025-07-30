using ChatApp.Domain.Enums;

namespace ChatApp.Application.Common.Models;


public record MessageDto
{
    public Guid Id { get; init; }public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; }
    public Guid SenderId { get; init; }
    public string SenderUsername { get; init; } = string.Empty;
    public string? SenderDisplayName { get; init; }
    public Guid ChatId { get; init; }
    public DateTime SentAt { get; init; }
    public DateTime? EditedAt { get; init; }
    public bool IsDeleted { get; init; }

}