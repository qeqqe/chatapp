using ChatApp.Application.Common.Models;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Interfaces
{
    public interface IChatService
    {
    Task<ChatDto?> CreateChatAsync(string name, string? description, ChatType type, Guid createdById, CancellationToken cancellationToken = default);
    Task<bool> AddUserToChatAsync(Guid chatId, Guid userId, ParticipantRole role = ParticipantRole.Member, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatDto>> GetUserChatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ChatDto?> GetChatByIdAsync(Guid chatId, CancellationToken cancellationToken = default);
}
}