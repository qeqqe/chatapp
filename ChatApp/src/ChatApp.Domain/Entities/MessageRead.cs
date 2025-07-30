namespace ChatApp.Domain.Entities
{
    public class MessageRead
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        // navigation props

        public virtual Message Message { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}