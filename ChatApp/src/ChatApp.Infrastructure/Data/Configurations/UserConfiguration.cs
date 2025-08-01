using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
        .IsRequired()
        .HasMaxLength(100);

        builder.Property(u => u.Email)
        .IsRequired()
        .HasMaxLength(255);

        builder.Property(u => u.DisplayName)
        .HasMaxLength(255);

        builder.HasIndex(u => u.Username).IsUnique();

        builder.HasIndex(u => u.IsOnline);


        // Relations
        builder.HasMany(u => u.Messages)
        .WithOne(m => m.Sender)
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ChatParticipants)
        .WithOne(cp => cp.User)
        .HasForeignKey(cp => cp.UserId)
        .OnDelete(DeleteBehavior.Cascade);


    }
}