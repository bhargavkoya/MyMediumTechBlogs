using ChatApp.Api.Models;

namespace ChatApp.Api.Dtos
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public UserDto Sender { get; set; } = null!;
    }
}
