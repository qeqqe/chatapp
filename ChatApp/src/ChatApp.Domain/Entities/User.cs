using System.ComponentModel.DataAnnotations;

namespace ChatApp.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? DisplayName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSeen { get; set; }
        public bool IsOnline { get; set; } = false;

        // navigation props
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        public virtual ICollection<ChatParticipant>
    ChatParticipants
        { get; set; } = new List<ChatParticipant>();

    }
}