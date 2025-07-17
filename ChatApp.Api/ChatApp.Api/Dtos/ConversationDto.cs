using ChatApp.Api.Models;

namespace ChatApp.Api.Dtos
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ConversationType Type { get; set; }
        public DateTime LastActivityAt { get; set; }
        public List<UserDto> Participants { get; set; } = new();
        public MessageDto? LastMessage { get; set; }
    }
}
