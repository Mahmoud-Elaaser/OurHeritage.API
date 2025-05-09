using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Service.DTOs.ChatDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private static readonly Dictionary<int, string> _userConnections = new Dictionary<int, string>();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string connectionId = Context.ConnectionId;

            // Store the connection
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = connectionId;
                }
                else
                {
                    _userConnections.Add(userId, connectionId);
                }
            }

            // Get all conversations this user is part of and join those groups
            var conversations = await _chatService.GetUserConversationsAsync(userId);
            foreach (var conversation in conversations.Items)
            {
                await Groups.AddToGroupAsync(connectionId, $"conversation_{conversation.Id}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Remove the connection
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections.Remove(userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto dto)
        {
            int senderId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var message = await _chatService.SendMessageAsync(dto, senderId);

            if (message != null)
            {
                // Broadcast to all users in the conversation
                await Clients.Group($"conversation_{dto.ConversationId}").SendAsync("ReceiveMessage", new MessageDto
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    Type = message.Type,
                    Attachment = message.Attachment,
                    Sender = new UserPreviewDto
                    {
                        Id = senderId,
                        FirstName = Context.User.FindFirstValue(ClaimTypes.GivenName),
                        LastName = Context.User.FindFirstValue(ClaimTypes.Surname),
                        ProfilePicture = Context.User.FindFirstValue("ProfilePicture")
                    },
                    IsRead = false,
                    ReadBy = new List<UserPreviewDto>(),
                    // Include reply information even for regular messages (will be null)
                    ReplyToMessageId = null,
                    ReplyToMessage = null
                });

                // Update the conversation timestamp for all users
                await Clients.Group($"conversation_{dto.ConversationId}").SendAsync("ConversationUpdated", dto.ConversationId);
            }
        }

        public async Task ReplyToMessage(ReplyMessageDto dto)
        {
            int senderId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var message = await _chatService.ReplyToMessageAsync(dto, senderId);

            if (message != null)
            {
                // Fetch the original message using pagination
                var paginatedMessages = await _chatService.GetConversationMessagesAsync(dto.ConversationId, senderId, page: 1, pageSize: 100);
                var originalMessage = paginatedMessages.Items.FirstOrDefault(m => m.Id == dto.ReplyToMessageId);

                var replyPreview = originalMessage != null ? new ReplyPreviewDto
                {
                    Id = originalMessage.Id,
                    Content = originalMessage.Content,
                    Type = originalMessage.Type,
                    Sender = new UserPreviewDto
                    {
                        Id = originalMessage.Sender.Id,
                        FirstName = originalMessage.Sender.FirstName,
                        LastName = originalMessage.Sender.LastName,
                        ProfilePicture = originalMessage.Sender.ProfilePicture
                    }
                } : null;

                // Broadcast to all users in the conversation
                await Clients.Group($"conversation_{dto.ConversationId}").SendAsync("ReceiveMessage", new MessageDto
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    Type = message.Type,
                    Attachment = message.Attachment,
                    Sender = new UserPreviewDto
                    {
                        Id = senderId,
                        FirstName = Context.User.FindFirstValue(ClaimTypes.GivenName),
                        LastName = Context.User.FindFirstValue(ClaimTypes.Surname),
                        ProfilePicture = Context.User.FindFirstValue("ProfilePicture")
                    },
                    IsRead = false,
                    ReadBy = new List<UserPreviewDto>(),
                    ReplyToMessageId = dto.ReplyToMessageId,
                    ReplyToMessage = replyPreview
                });

                // Notify all users in the conversation to update their UI
                await Clients.Group($"conversation_{dto.ConversationId}").SendAsync("ConversationUpdated", dto.ConversationId);
            }
        }


        public async Task JoinConversation(JoinConversationDto dto)
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Try to join the conversation in the database
            var conversationUser = await _chatService.JoinConversationAsync(dto.ConversationId, userId);

            if (conversationUser != null)
            {
                // Add user to the SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{dto.ConversationId}");

                // Notify other users in the conversation
                var userInfo = new UserPreviewDto
                {
                    Id = userId,
                    FirstName = Context.User.FindFirstValue(ClaimTypes.GivenName),
                    LastName = Context.User.FindFirstValue(ClaimTypes.Surname),
                    ProfilePicture = Context.User.FindFirstValue("ProfilePicture")
                };

                await Clients.Group($"conversation_{dto.ConversationId}").SendAsync("UserJoinedConversation", dto.ConversationId, userInfo);

                // Send confirmation to the client
                await Clients.Caller.SendAsync("JoinedConversation", dto.ConversationId);
            }
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task MarkAsRead(int messageId)
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _chatService.MarkMessageAsReadAsync(messageId, userId);

            // Get the message to find which conversation it belongs to
            var messageResponse = await _chatService.GetConversationMessagesAsync(0, userId, 1, 1);

            // Check if there are messages in the response
            if (messageResponse != null && messageResponse.Items.Any())
            {
                var message = messageResponse.Items.FirstOrDefault(); // Access the first message

                if (message != null)
                {
                    // Notify others that this user has read the message
                    await Clients.Group($"conversation_{message.ConversationId}").SendAsync("MessageRead", messageId, userId);
                }
            }
        }



        private string TruncateContent(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "...";
        }
    }
}
