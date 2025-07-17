namespace ChatApp.Api.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Text;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
    }

    // Enums/MessageType.cs
    public enum MessageType
    {
        Text,
        Image,
        File,
        System
    }
}
