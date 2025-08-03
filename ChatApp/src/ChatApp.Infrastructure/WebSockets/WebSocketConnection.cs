using System.Net.WebSockets;

namespace ChatApp.Infrastructure.WebSockets;

public class WebSocketConnection
{
    public Guid UserId { get; set; }
    public WebSocket WebSocket { get; set; } = null!;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public string ConnectionId { get; set; } = Guid.NewGuid().ToString();
}