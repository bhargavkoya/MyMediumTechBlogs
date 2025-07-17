using ChatApp.Api.Models;

namespace ChatApp.Api.RequestsEntities
{
    public class SendMessageRequest
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Text;
    }
}
