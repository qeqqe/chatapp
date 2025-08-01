using System.Data;
using System.Reflection;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Data;

public class ChatDbContext : DbContext
{

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Chat> Chats { get; set; } = null!;

    public DbSet<Message> Messages { get; set; } = null!;

    public DbSet<ChatParticipant> ChatParticipants { get; set; } = null!;

    public DbSet<MessageRead> MessageReads { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Message>().HasQueryFilter(m => m.IsDeleted);

        modelBuilder.Entity<ChatParticipant>().HasQueryFilter(cp => cp.IsActive);

    }

    public override async Task<int>
    SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries) {
            if (entry.Entity is Chat chat)
            {
                chat.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

}