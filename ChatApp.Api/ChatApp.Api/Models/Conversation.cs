namespace ChatApp.Api.Models
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ConversationType Type { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User CreatedBy { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    }

    public enum ConversationType
    {
        OneOnOne,
        Group,
        Department,
        Public
    }
}
