using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.ChatDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Conversation> GetConversationByIdAsync(int conversationId, int userId)
        {
            // First check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(conversationId, userId);
            if (!isUserInConversation)
            {
                return null;
            }

            return await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.ReadByUsers)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }




        public async Task<Conversation> CreateConversationAsync(CreateConversationDto dto, int creatorId)
        {
            // Check if it's a direct message (between 2 users) and if so, check if a conversation already exists
            if (!dto.IsGroup && dto.ParticipantIds.Count == 1)
            {
                int otherUserId = dto.ParticipantIds[0];

                var existingConversation = await _context.Conversations
                    .Include(c => c.Participants)
                    .Where(c => !c.IsGroup)
                    .Where(c => c.Participants.Count() == 2)
                    .Where(c => c.Participants.Any(p => p.UserId == creatorId))
                    .Where(c => c.Participants.Any(p => p.UserId == otherUserId))
                    .FirstOrDefaultAsync();

                if (existingConversation != null)
                {
                    return existingConversation;
                }
            }

            // Create new conversation
            var conversation = new Conversation
            {
                Title = dto.Title,
                IsGroup = dto.IsGroup,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var allParticipantIds = dto.ParticipantIds.Append(creatorId).Distinct();

            foreach (var userId in allParticipantIds)
            {
                // Avoid tracking conflicts by checking first
                var existing = await _context.ConversationUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ConversationId == conversation.Id);

                if (existing == null)
                {
                    _context.ConversationUsers.Add(new ConversationUser
                    {
                        ConversationId = conversation.Id,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return conversation;
        }


        public async Task<Message> SendMessageAsync(SendMessageDto dto, int senderId)
        {
            // Check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(dto.ConversationId, senderId);
            if (!isUserInConversation)
            {
                return null;
            }

            // Create message
            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = senderId,
                Content = dto.Content,
                Type = dto.Type,
                Attachment = dto.Attachment,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);

            // Update conversation's UpdatedAt
            var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
            conversation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetConversationMessagesAsync(int conversationId, int userId, int page = 1, int pageSize = 20)
        {
            // Check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(conversationId, userId);
            if (!isUserInConversation)
            {
                return null;
            }

            // Get paginated messages ordered by sent date
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReadByUsers)
                    .ThenInclude(r => r.User)
                // Include replied-to messages with their senders
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task MarkMessageAsReadAsync(int messageId, int userId)
        {
            var message = await _context.Messages
                .Include(m => m.ReadByUsers)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null) return;

            // Check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(message.ConversationId, userId);
            if (!isUserInConversation) return;

            // Check if message is already marked as read by this user
            if (message.ReadByUsers.Any(r => r.UserId == userId)) return;

            // Mark message as read
            _context.MessageReads.Add(new MessageRead
            {
                MessageId = messageId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllMessagesAsReadAsync(int conversationId, int userId)
        {
            // Check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(conversationId, userId);
            if (!isUserInConversation) return;

            // Get all unread messages in this conversation that weren't sent by this user
            var unreadMessages = await _context.Messages
                .Include(m => m.ReadByUsers)
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.SenderId != userId)
                .Where(m => !m.ReadByUsers.Any(r => r.UserId == userId))
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                _context.MessageReads.Add(new MessageRead
                {
                    MessageId = message.Id,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }




        public async Task<(int Count, List<Message> Messages)> GetUnreadMessagesAsync(int userId)
        {
            // Get conversations this user is part of
            var conversationIds = await _context.ConversationUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ConversationId)
                .ToListAsync();

            // Get unread messages in these conversations
            var unreadMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Conversation)
                .Include(m => m.ReadByUsers)
                    .ThenInclude(r => r.User)
                // Include replied-to messages with their senders
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .Where(m => conversationIds.Contains(m.ConversationId))
                .Where(m => m.SenderId != userId) // Don't count user's own messages
                .Where(m => !m.ReadByUsers.Any(r => r.UserId == userId))
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return (unreadMessages.Count, unreadMessages);
        }



        public async Task<bool> IsUserInConversationAsync(int conversationId, int userId)
        {
            return await _context.ConversationUsers
                .AnyAsync(cu => cu.ConversationId == conversationId && cu.UserId == userId);
        }

        public async Task<Message> ReplyToMessageAsync(ReplyMessageDto dto, int senderId)
        {
            // Check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(dto.ConversationId, senderId);
            if (!isUserInConversation)
            {
                return null;
            }

            // Verify the referenced message exists and belongs to the same conversation
            var referencedMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == dto.ReplyToMessageId && m.ConversationId == dto.ConversationId);

            if (referencedMessage == null)
            {
                return null; // The referenced message doesn't exist or is not in this conversation
            }

            // Create message with reference to the original message
            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = senderId,
                Content = dto.Content,
                Type = dto.Type,
                Attachment = dto.Attachment,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                //ReplyToMessageId = dto.ReplyToMessageId
            };

            _context.Messages.Add(message);

            // Update conversation's UpdatedAt
            var conversation = await _context.Conversations.FindAsync(dto.ConversationId);
            conversation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<ConversationUser> JoinConversationAsync(int conversationId, int userId)
        {
            // Check if the conversation exists
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return null;
            }

            // Check if user is already in the conversation
            var existingMembership = await _context.ConversationUsers
                .FirstOrDefaultAsync(cu => cu.ConversationId == conversationId && cu.UserId == userId);

            if (existingMembership != null)
            {
                return existingMembership; // User is already a member
            }

            // Add user to conversation
            var conversationUser = new ConversationUser
            {
                ConversationId = conversationId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.ConversationUsers.Add(conversationUser);
            await _context.SaveChangesAsync();

            return conversationUser;
        }
    }
}
