using ChatApp.Api.Models;

namespace ChatApp.Api.RequestsEntities
{
    public class CreateConversationRequest
    {
        public string Name { get; set; } = string.Empty;
        public ConversationType Type { get; set; }
        public Guid CreatedById { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
    }
}
