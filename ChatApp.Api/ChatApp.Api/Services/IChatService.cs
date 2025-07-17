using ChatApp.Api.Dtos;
using ChatApp.Api.RequestsEntities;

namespace ChatApp.Api.Services
{
    public interface IChatService
    {
        Task<List<ConversationDto>> GetUserConversationsAsync(Guid userId);
        Task<ConversationDto> CreateConversationAsync(CreateConversationRequest request);
        Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid userId, int page = 1, int pageSize = 50);
        Task AddParticipantAsync(Guid conversationId, Guid userId, Guid requestingUserId);
        Task RemoveParticipantAsync(Guid conversationId, Guid userId, Guid requestingUserId);
        Task<MessageDto> SendMessageAsync(SendMessageRequest request, Guid senderId);
        Task MarkMessageAsReadAsync(Guid messageId, Guid userId);
    }
}
