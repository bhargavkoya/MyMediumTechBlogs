// Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ChatApp.Api.Data;
using ChatApp.Api.Models;
using ChatApp.Api.Services;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatDbContext _context;
    private readonly IChatService _chatService;

    public ChatHub(ChatDbContext context, IChatService chatService)
    {
        _context = context;
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            // Store connection
            var connection = new UserConnection
            {
                UserId = userId.Value,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow
            };

            _context.UserConnections.Add(connection);

            // Update user online status
            var user = await _context.Users.FindAsync(userId.Value);
            if (user != null)
            {
                user.IsOnline = true;
                user.LastSeen = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Join user's conversation groups
            var userConversations = await _context.ConversationParticipants
                .Where(cp => cp.UserId == userId.Value)
                .Select(cp => cp.ConversationId.ToString())
                .ToListAsync();

            foreach (var conversationId in userConversations)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            }

            // Notify contacts that user is online
            await NotifyContactsUserStatusChanged(userId.Value, true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            // Remove connection
            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

            if (connection != null)
            {
                _context.UserConnections.Remove(connection);
            }

            // Check if user has other active connections
            var hasOtherConnections = await _context.UserConnections
                .AnyAsync(c => c.UserId == userId.Value && c.ConnectionId != Context.ConnectionId);

            if (!hasOtherConnections)
            {
                // Update user offline status
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Notify contacts that user is offline
                await NotifyContactsUserStatusChanged(userId.Value, false);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = GetUserId();
        if (userId == null) return;

        // Verify user is participant in conversation
        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId.Value);

        if (!isParticipant) return;

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId.Value,
            Content = content,
            Type = MessageType.Text,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        // Update conversation last activity
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.LastActivityAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Load sender information
        var sender = await _context.Users
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.Id, u.Username, u.FirstName, u.LastName, u.ProfileImageUrl })
            .FirstOrDefaultAsync();

        var messageDto = new
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            Content = message.Content,
            SentAt = message.SentAt,
            Sender = sender
        };

        // Send to conversation group
        await Clients.Group($"conversation_{conversationId}")
            .SendAsync("ReceiveMessage", messageDto);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        // Verify user is participant
        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId.Value);

        if (isParticipant)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task StartTyping(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var user = await _context.Users
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.Id, u.Username, u.FirstName, u.LastName })
            .FirstOrDefaultAsync();

        await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
            .SendAsync("UserStartedTyping", user);
    }

    public async Task StopTyping(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var user = await _context.Users
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.Id, u.Username, u.FirstName, u.LastName })
            .FirstOrDefaultAsync();

        await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
            .SendAsync("UserStoppedTyping", user);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task NotifyContactsUserStatusChanged(Guid userId, bool isOnline)
    {
        // Get user's conversations
        var conversationIds = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.ConversationId.ToString())
            .ToListAsync();

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Username, u.FirstName, u.LastName, u.IsOnline })
            .FirstOrDefaultAsync();

        // Notify all conversation groups
        foreach (var conversationId in conversationIds)
        {
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("UserStatusChanged", user);
        }
    }
}
