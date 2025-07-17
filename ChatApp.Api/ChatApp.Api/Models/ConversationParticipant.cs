namespace ChatApp.Api.Models
{
    public class ConversationParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public Guid UserId { get; set; }
        public ParticipantRole Role { get; set; } = ParticipantRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastReadAt { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User User { get; set; } = null!;
    }

    public enum ParticipantRole
    {
        Member,
        Admin,
        Owner
    }
}
