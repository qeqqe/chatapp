using ChatApp.Application.Common.Models;
using ChatApp.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ChatApp.Infrastructure.WebSockets;

public class WebSocketService : IWebSocketService
{
    private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new();
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _chatConnections = new();
    private readonly ILogger<WebSocketService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public WebSocketService(ILogger<WebSocketService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task AddConnectionAsync(Guid userId, WebSocket webSocket)
    {
        var connection = new WebSocketConnection
        {
            UserId = userId,
            WebSocket = webSocket
        };

        _connections.TryAdd(userId, connection);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        await SubscribeUserToChatsAsync(userId);

        _logger.LogInformation("User {UserId} connected via WebSocket", userId);

        await NotifyUserStatusChangeAsync(userId, true);
    }

    public async Task RemoveConnectionAsync(Guid userId)
    {
        if (_connections.TryRemove(userId, out var connection))
        {
            if (connection.WebSocket.State == WebSocketState.Open)
            {
                await connection.WebSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed",
                    CancellationToken.None);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null)
            {
                user.IsOnline = false;
                user.LastSeen = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            await UnsubscribeUserFromChatsAsync(userId);

            _logger.LogInformation("User {UserId} disconnected from WebSocket", userId);

            await NotifyUserStatusChangeAsync(userId, false);
        }
    }

    public async Task SendMessageToUserAsync(Guid userId, WebSocketMessage message)
    {
        if (_connections.TryGetValue(userId, out var connection) && 
            connection.WebSocket.State == WebSocketState.Open)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                
                await connection.WebSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {UserId}", userId);
                await RemoveConnectionAsync(userId);
            }
        }
    }

    public async Task SendMessageToChatAsync(Guid chatId, WebSocketMessage message, Guid? excludeUserId = null)
    {
        if (!_chatConnections.TryGetValue(chatId, out var userIds))
            return;

        var tasks = userIds
            .Where(userId => userId != excludeUserId)
            .Select(userId => SendMessageToUserAsync(userId, message));

        await Task.WhenAll(tasks);
    }

    public async Task SendMessageToAllAsync(WebSocketMessage message, Guid? excludeUserId = null)
    {
        var tasks = _connections
            .Where(kvp => kvp.Key != excludeUserId)
            .Select(kvp => SendMessageToUserAsync(kvp.Key, message));

        await Task.WhenAll(tasks);
    }

    public Task<bool> IsUserOnlineAsync(Guid userId)
    {
        return Task.FromResult(_connections.ContainsKey(userId));
    }

    public Task<IEnumerable<Guid>> GetOnlineUsersAsync()
    {
        return Task.FromResult(_connections.Keys.AsEnumerable());
    }

    private async Task SubscribeUserToChatsAsync(Guid userId)
    {
        var participations = await _unitOfWork.ChatParticipants
            .FindAsync(cp => cp.UserId == userId && cp.IsActive);

        foreach (var participation in participations)
        {
            _chatConnections.AddOrUpdate(
                participation.ChatId,
                new HashSet<Guid> { userId },
                (key, existingSet) =>
                {
                    existingSet.Add(userId);
                    return existingSet;
                });
        }
    }

    private Task UnsubscribeUserFromChatsAsync(Guid userId)
    {
        foreach (var chatConnection in _chatConnections)
        {
            chatConnection.Value.Remove(userId);
            
            // Remove empty chat connections
            if (chatConnection.Value.Count == 0)
            {
                _chatConnections.TryRemove(chatConnection.Key, out _);
            }
        }

        return Task.CompletedTask;
    }

    private async Task NotifyUserStatusChangeAsync(Guid userId, bool isOnline)
    {
        var statusMessage = new WebSocketMessage
        {
            Type = "USER_STATUS_CHANGE",
            Data = JsonSerializer.SerializeToElement(new
            {
                UserId = userId,
                IsOnline = isOnline,
                Timestamp = DateTime.UtcNow
            })
        };

        await SendMessageToAllAsync(statusMessage, userId);
    }
}
