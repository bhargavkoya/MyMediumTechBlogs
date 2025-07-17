using AutoMapper;
using ChatApp.Api.Data;
using ChatApp.Api.Dtos;
using ChatApp.Api.Models;
using ChatApp.Api.RequestsEntities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatDbContext _context;
        private readonly IMapper _mapper;

        public ChatService(ChatDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ConversationDto>> GetUserConversationsAsync(Guid userId)
        {
            var conversations = await _context.ConversationParticipants
                .Where(cp => cp.UserId == userId)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Select(cp => new ConversationDto
                {
                    Id = cp.Conversation.Id,
                    Name = cp.Conversation.Name,
                    Type = cp.Conversation.Type,
                    LastActivityAt = cp.Conversation.LastActivityAt,
                    Participants = cp.Conversation.Participants.Select(p => new UserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        FirstName = p.User.FirstName,
                        LastName = p.User.LastName,
                        IsOnline = p.User.IsOnline,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }).ToList(),
                    LastMessage = cp.Conversation.Messages.FirstOrDefault() != null ?
                        new MessageDto
                        {
                            Id = cp.Conversation.Messages.First().Id,
                            Content = cp.Conversation.Messages.First().Content,
                            SentAt = cp.Conversation.Messages.First().SentAt,
                            Sender = new UserDto
                            {
                                Id = cp.Conversation.Messages.First().Sender.Id,
                                Username = cp.Conversation.Messages.First().Sender.Username,
                                FirstName = cp.Conversation.Messages.First().Sender.FirstName,
                                LastName = cp.Conversation.Messages.First().Sender.LastName
                            }
                        } : null
                })
                .OrderByDescending(c => c.LastActivityAt)
                .ToListAsync();

            return conversations;
        }

        public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid userId, int page = 1, int pageSize = 50)
        {
            // Verify user is participant
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant)
                throw new UnauthorizedAccessException("User is not a participant in this conversation");

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    Type = m.Type,
                    SentAt = m.SentAt,
                    EditedAt = m.EditedAt,
                    Sender = new UserDto
                    {
                        Id = m.Sender.Id,
                        Username = m.Sender.Username,
                        FirstName = m.Sender.FirstName,
                        LastName = m.Sender.LastName,
                        ProfileImageUrl = m.Sender.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return messages.OrderBy(m => m.SentAt).ToList();
        }

        public async Task<ConversationDto> CreateConversationAsync(CreateConversationRequest request)
        {
            var conversation = new Conversation
            {
                Name = request.Name,
                Type = request.Type,
                CreatedById = request.CreatedById,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };
            _context.Conversations.Add(conversation);

            foreach (var userId in request.ParticipantIds)
            {
                _context.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationId = conversation.Id,
                    UserId = userId,
                    Role = userId == request.CreatedById ? ParticipantRole.Owner : ParticipantRole.Member,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ConversationDto>(conversation);
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageRequest request, Guid senderId)
        {
            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = senderId,
                Content = request.Content,
                Type = request.Type,
                SentAt = DateTime.UtcNow
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return _mapper.Map<MessageDto>(message);
        }

        public async Task MarkMessageAsReadAsync(Guid messageId, Guid userId)
        {
            var status = await _context.MessageStatuses
                .FirstOrDefaultAsync(ms => ms.MessageId == messageId && ms.UserId == userId);

            if (status != null)
            {
                status.Status = "read";
                status.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public Task AddParticipantAsync(Guid conversationId, Guid userId, Guid requestingUserId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveParticipantAsync(Guid conversationId, Guid userId, Guid requestingUserId)
        {
            throw new NotImplementedException();
        }
    }
}
