using System.Text.Json;
using ChatApp.Application.Common.Models;
using ChatApp.Application.Features.Messages;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;

namespace ChatApp.Application.Services;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebSocketService _webSocketService;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IUnitOfWork unitOfWork,
        IWebSocketService webSocketService,
        ILogger<MessageService> logger
    )
    {
        _unitOfWork = unitOfWork;
        _webSocketService = webSocketService;
        _logger = logger;
    }

    public async Task<CreateMessageResult> CreateMessageAsync(CreateMessageCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var participation = await _unitOfWork.ChatParticipants
            .FindAsync(cp => cp.UserId == command.SenderId
            && cp.ChatId == command.ChatId && cp.IsActive, cancellationToken);

            if (!participation.Any())
            {
                return new CreateMessageResult
                {
                    Success = false,
                    ErrorMessage = "User is not a participant of this chat"
                };
            }
            var message = new Message
            {
                Content = command.Content,
                Type = command.Type,
                SenderId = command.ChatId
            };

            await _unitOfWork.Messages.AddAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageWithSender = await GetMessageWithSenderAsync(message.Id, cancellationToken);
            if (messageWithSender == null)
            {
                throw new InvalidOperationException("Failed to retrieve created message");
            }

            var messageDto = MapToMessageDto(messageWithSender);

            // Send real-time notification
            var wsMessage = new WebSocketMessage
            {
                Type = "NEW_MESSAGE",
                Data = JsonSerializer.SerializeToElement(messageDto)
            };

            await _webSocketService.SendMessageToChatAsync(command.ChatId, wsMessage, command.SenderId);

            return new CreateMessageResult
            {
                Message = messageDto,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating message for chat {ChatId}", command.ChatId);
            return new CreateMessageResult
            {
                Success = false,
                ErrorMessage = "An error occurred while creating the message"
            };
        }

    }


    public async Task<IEnumerable<MessageDto>> GetChatMessagesAsync(Guid chatId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        // Get messages with pagination, ordered by most recent first
        var messages = await _unitOfWork.Messages
            .FindAsync(m => m.ChatId == chatId, cancellationToken);

        var pagedMessages = messages
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var messagesWithSenders = new List<MessageDto>();
        
        foreach (var message in pagedMessages)
        {
            var messageWithSender = await GetMessageWithSenderAsync(message.Id, cancellationToken);
            if (messageWithSender != null)
            {
                messagesWithSenders.Add(MapToMessageDto(messageWithSender));
            }
        }

        return messagesWithSenders.OrderBy(m => m.SentAt);
    }

    public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingRead = await _unitOfWork.MessageRead
                .FindAsync(mr => mr.MessageId == messageId && mr.UserId == userId, cancellationToken);

            if (existingRead.Any())
                return true;

            var messageRead = new MessageRead
            {
                MessageId = messageId,
                UserId = userId
            };

            await _unitOfWork.MessageRead.AddAsync(messageRead, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read for user {UserId}", messageId, userId);
            return false;
        }
    }

    private async Task<Message?> GetMessageWithSenderAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var messages = await _unitOfWork.Messages.FindAsync(m => m.Id == messageId, cancellationToken);
        var message = messages.FirstOrDefault();
        
        if (message != null)
        {
            var senders = await _unitOfWork.Users.FindAsync(u => u.Id == message.SenderId, cancellationToken);
            message.Sender = senders.FirstOrDefault()!;
        }
        
        return message;
    }

    private static MessageDto MapToMessageDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            Type = message.Type,
            SenderId = message.SenderId,
            SenderUsername = message.Sender.Username,
            SenderDisplayName = message.Sender.DisplayName,
            ChatId = message.ChatId,
            SentAt = message.SentAt,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted
        };
    }
}