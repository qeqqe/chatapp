using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("Chats");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
        .IsRequired()
        .HasMaxLength(255);

        builder.Property(c => c.Description)
        .IsRequired()
        .HasMaxLength(500);

        builder.Property(c => c.Type)
        .IsRequired()
        .HasConversion<int>();

        builder.HasIndex(c => c.CreatedBy);
        builder.HasIndex(c => c.Type);

        // Relations

        builder.HasOne(c => c.CreatedBy)
        .WithMany()
        .HasForeignKey(c => c.CreatedById)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Messages)
        .WithOne(m => m.Chat)
        .HasForeignKey(m => m.ChatId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Participants)
        .WithOne(cp => cp.Chat)
        .HasForeignKey(cp => cp.ChatId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}