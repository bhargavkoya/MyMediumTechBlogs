namespace ChatApp.Api.Models
{
    public class UserConnection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
