
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using ChatApp.Domain.Interfaces;

namespace ChatApp.Application.Services;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
 public async Task<ChatDto?> CreateChatAsync(string name, string? description, ChatType type, Guid createdById, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var chat = new Chat
            {
                Name = name,
                Description = description,
                Type = type,
                CreatedById = createdById
            };

            await _unitOfWork.Chats.AddAsync(chat, cancellationToken);

            // Add creator as owner
            var participation = new ChatParticipant
            {
                UserId = createdById,
                ChatId = chat.Id,
                Role = ParticipantRole.Owner
            };

            await _unitOfWork.ChatParticipants.AddAsync(participation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return await GetChatByIdAsync(chat.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating chat");
            return null;
        }
    }

    public async Task<bool> AddUserToChatAsync(Guid chatId, Guid userId, ParticipantRole role = ParticipantRole.Member, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user is already a participant
            var existingParticipation = await _unitOfWork.ChatParticipants
                .FindAsync(cp => cp.ChatId == chatId && cp.UserId == userId && cp.IsActive, cancellationToken);

            if (existingParticipation.Any())
                return true;

            var participation = new ChatParticipant
            {
                UserId = userId,
                ChatId = chatId,
                Role = role
            };

            await _unitOfWork.ChatParticipants.AddAsync(participation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to chat {ChatId}", userId, chatId);
            return false;
        }
    }

    public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participations = await _unitOfWork.ChatParticipants
            .FindAsync(cp => cp.UserId == userId && cp.IsActive, cancellationToken);

        var chatIds = participations.Select(p => p.ChatId).ToList();
        var chats = new List<ChatDto>();

        foreach (var chatId in chatIds)
        {
            var chat = await GetChatByIdAsync(chatId, cancellationToken);
            if (chat != null)
            {
                chats.Add(chat);
            }
        }

        return chats.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
    }

    public async Task<ChatDto?> GetChatByIdAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        var chats = await _unitOfWork.Chats.FindAsync(c => c.Id == chatId, cancellationToken);
        var chat = chats.FirstOrDefault();

        if (chat == null)
            return null;

        // Get creator
        var creators = await _unitOfWork.Users.FindAsync(u => u.Id == chat.CreatedById, cancellationToken);
        var creator = creators.FirstOrDefault();

        // Get participant count
        var participants = await _unitOfWork.ChatParticipants
            .FindAsync(cp => cp.ChatId == chatId && cp.IsActive, cancellationToken);
        var participantCount = participants.Count();

        // Get last message
        var messages = await _unitOfWork.Messages
            .FindAsync(m => m.ChatId == chatId, cancellationToken);
        var lastMessage = messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

        MessageDto? lastMessageDto = null;
        if (lastMessage != null)
        {
            var senders = await _unitOfWork.Users.FindAsync(u => u.Id == lastMessage.SenderId, cancellationToken);
            var sender = senders.FirstOrDefault();
            if (sender != null)
            {
                lastMessageDto = new MessageDto
                {
                    Id = lastMessage.Id,
                    Content = lastMessage.Content,
                    Type = lastMessage.Type,
                    SenderId = lastMessage.SenderId,
                    SenderUsername = sender.Username,
                    SenderDisplayName = sender.DisplayName,
                    ChatId = lastMessage.ChatId,
                    SentAt = lastMessage.SentAt,
                    EditedAt = lastMessage.EditedAt,
                    IsDeleted = lastMessage.IsDeleted
                };
            }
        }

        return new ChatDto
        {
            Id = chat.Id,
            Name = chat.Name,
            Description = chat.Description,
            Type = chat.Type,
            CreatedById = chat.CreatedById,
            CreatedByUsername = creator?.Username ?? "Unknown",
            CreatedAt = chat.CreatedAt,
            UpdatedAt = chat.UpdatedAt,
            ParticipantCount = participantCount,
            LastMessage = lastMessageDto
        };
    }
}