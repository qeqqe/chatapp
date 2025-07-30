using System.ComponentModel.DataAnnotations;

namespace ChatApp.Domain.Entities
{
    public class Message
    {
        public Guid Id = Guid.NewGuid();

        [Required]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;

        public Guid SenderId { get; set; }
        public Guid ChatId { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // navigation props

        public virtual User Sender { get; set; } = null!;
        public virtual Chat Chat { get; set; } = null!;
        public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
    }
}