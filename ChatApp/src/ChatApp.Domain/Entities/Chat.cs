
using System.ComponentModel.DataAnnotations;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities
{
    public class Chat
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ChatType Type { get; set; } = ChatType.Group;

        public Guid CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        // navigation props

        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        
        

        
    }
}