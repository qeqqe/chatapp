using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
        .IsRequired()
        .HasColumnType("text");

        builder.Property(m => m.Type)
        .IsRequired()
        .HasConversion<int>();

        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.ChatId);
        builder.HasIndex(m => m.SentAt);
        builder.HasIndex(m => m.IsDeleted);

        // relations

        builder.HasOne(m => m.Sender)
        .WithMany(u => u.Messages)
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Chat)
        .WithMany(u => u.Messages)
        .HasForeignKey(m => m.ChatId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.MessageReads)
        .WithOne(mr => mr.Message)
        .HasForeignKey(mr => mr.MessageId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}