using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities
{
    public class ChatParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ChatId { get; set; }
        public ParticipantRole Role { get; set; } =
        ParticipantRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public bool IsActive { get; set; } = true;

        //navigation props

        public virtual User User { get; set; } = null!;
        public virtual Chat Chat { get; set; } = null!;
    }
}