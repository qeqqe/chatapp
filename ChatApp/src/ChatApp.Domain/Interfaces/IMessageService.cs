using ChatApp.Application.Common.Models;
using ChatApp.Application.Features.Messages;

public interface IMessageService
{
    Task<CreateMessageResult> CreateMessageAsync(CreateMessageCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageDto>> GetChatMessagesAsync(Guid chatId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
}