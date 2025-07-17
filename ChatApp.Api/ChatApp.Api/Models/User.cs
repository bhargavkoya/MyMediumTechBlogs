namespace ChatApp.Api.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<ConversationParticipant> Conversations { get; set; } = new List<ConversationParticipant>();
        public ICollection<UserConnection> Connections { get; set; } = new List<UserConnection>();
    }
}
