using System.Net.WebSockets;
using ChatApp.Application.Common.Models;

namespace ChatApp.Domain.Interfaces;

public interface IWebSocketService
{
    Task AddConnectionAsync(Guid userId, WebSocket webSocket);
    Task RemoveConnectionAsync(Guid userId);
    Task SendMessageToChatAsunc(Guid userId, WebSocketMessage message, Guid? excludeUserId = null);
    Task SendMessageToUserAsunc(Guid userId, WebSocketMessage message);

    Task SendMessageToAllAsync(WebSocketMessage message, Guid? excludeUserId = null);

    Task<bool> IsUserOnlineAsync(Guid userId);
    Task<IEnumerable<Guid>> GetOnlineUsrsAsync();
    
}