using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Chat> Chats { get; }
    IRepository<Message> Messages { get; }
    IRepository<ChatParticipant> ChatParticipants { get; }
    IRepository<MessageRead> MessageRead { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

}