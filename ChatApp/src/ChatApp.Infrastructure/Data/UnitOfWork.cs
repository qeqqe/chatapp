using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using ChatApp.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatApp.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ChatDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IRepository<User>? _users;
    private IRepository<Chat>? _chats;
    private IRepository<Message>? _messages;
    private IRepository<ChatParticipant>? _chatParticipants;
    private IRepository<MessageRead>? _messageReads;

    public UnitOfWork(ChatDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users =>
        _users ??= new Repository<User>(_context);

    public IRepository<Chat> Chats =>
        _chats ??= new Repository<Chat>(_context);

    public IRepository<Message> Messages =>
        _messages ??= new Repository<Message>(_context);

    public IRepository<ChatParticipant> ChatParticipants =>
        _chatParticipants ??= new Repository<ChatParticipant>(_context);

    public IRepository<MessageRead> MessageReads =>
        _messageReads ??= new Repository<MessageRead>(_context);

    public IRepository<MessageRead> MessageRead => throw new NotImplementedException();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
