using ChatApp.Domain.Enums;

namespace ChatApp.Application.Common.Models;

public record ChatDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ChatType Type { get; init; }
    public Guid CreatedById { get; init; }
    public string CreatedByUsername { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int ParticipantCount { get; init; }
    public MessageDto? LastMessage { get; init; }

}