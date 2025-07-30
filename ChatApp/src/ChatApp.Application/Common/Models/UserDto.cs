namespace ChatApp.Application.Common.Models;

public record UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastSeenAt { get; init; }
    public bool IsOnline { get; init; }
}
