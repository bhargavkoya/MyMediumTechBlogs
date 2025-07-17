namespace ChatApp.Api.Models
{
    public class MessageStatus
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key to the Message entity
        public Guid MessageId { get; set; }
        public Message Message { get; set; } = null!;

        // Foreign key to the User entity (the recipient)
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Status value: "sending", "sent", "delivered", "read", "failed", "bounced"
        public string Status { get; set; } = "sent";

        // When the status was last updated
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
