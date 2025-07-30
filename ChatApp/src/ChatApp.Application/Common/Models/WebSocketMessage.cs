using System.Text.Json;

namespace ChatApp.Application.Common.Models;

public record WebSocketMessage
{
    public string Type { get; init; } = string.Empty;
    public JsonElement Data { get; init; }
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}