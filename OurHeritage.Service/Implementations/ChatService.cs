using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs.ChatDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;

        public ChatService(ApplicationDbContext context, IMapper mapper, IPaginationService paginationService)
        {
            _context = context;
            _mapper = mapper;
            _paginationService = paginationService;
        }

        public async Task<Message> SendMessageAsync(SendMessageDto dto, int senderId)
        {
            // Check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(dto.ConversationId, senderId);
            if (!isUserInConversation)
            {
                return null;
            }

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
            await UpdateConversationTimestamp(dto.ConversationId);

            await _context.SaveChangesAsync();
            return message;
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
                return null;
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
                ReplyToMessageId = dto.ReplyToMessageId
            };

            _context.Messages.Add(message);

            // Update conversation's UpdatedAt
            await UpdateConversationTimestamp(dto.ConversationId);

            await _context.SaveChangesAsync();
            return message;
        }


        public async Task<PaginationResponse<MessageDto>> GetConversationMessagesAsync(int conversationId, int userId, int page = 1, int pageSize = 10)
        {
            bool isUserInConversation = await IsUserInConversationAsync(conversationId, userId);
            if (!isUserInConversation)
            {
                return new PaginationResponse<MessageDto>
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0,
                    Items = new List<MessageDto>()
                };
            }

            var specParams = new SpecParams
            {
                PageIndex = page,
                PageSize = pageSize
            };


            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReadByUsers)
                    .ThenInclude(r => r.User)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();


            var paginatedResult = _paginationService.Paginate(allMessages, specParams, message =>
            {
                var dto = _mapper.Map<MessageDto>(message);
                dto.IsRead = message.ReadByUsers.Any(r => r.UserId == userId) || message.SenderId == userId;
                return dto;
            });

            return paginatedResult;
        }



        public async Task<PaginationResponse<MessageDto>> GetAllMessagesAsync(int userId, int page = 1, int pageSize = 10)
        {
            var specParams = new SpecParams
            {
                PageIndex = page,
                PageSize = pageSize
            };


            var conversationIds = await _context.ConversationUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ConversationId)
                .ToListAsync();


            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Conversation)
                .Include(m => m.ReadByUsers)
                    .ThenInclude(r => r.User)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .Where(m => conversationIds.Contains(m.ConversationId))
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();


            var paginatedResult = _paginationService.Paginate(allMessages, specParams, message =>
            {
                var dto = _mapper.Map<MessageDto>(message);
                dto.IsRead = message.ReadByUsers.Any(r => r.UserId == userId) || message.SenderId == userId;
                return dto;
            });

            return paginatedResult;
        }


        public async Task MarkMessageAsReadAsync(int messageId, int userId)
        {
            var message = await _context.Messages
                .Include(m => m.ReadByUsers)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null) return;

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

        public async Task<(int unreadCount, PaginationResponse<MessageDto> unreadMessages)> GetUnreadMessagesAsync(int userId, int page = 1, int pageSize = 10)
        {

            var conversationIds = await _context.ConversationUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ConversationId)
                .ToListAsync();

            // Filter unread messages
            var unreadMessagesQuery = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Conversation)
                .Include(m => m.ReadByUsers)
                    .ThenInclude(r => r.User)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .Where(m => conversationIds.Contains(m.ConversationId) &&
                            !m.ReadByUsers.Any(r => r.UserId == userId) &&
                            m.SenderId != userId)
                .OrderByDescending(m => m.SentAt);

            var unreadMessagesList = await unreadMessagesQuery.ToListAsync();


            var unreadMessageDtos = unreadMessagesList.Select(message =>
            {
                var dto = _mapper.Map<MessageDto>(message);
                dto.IsRead = false;
                return dto;
            });


            var paginated = _paginationService.Paginate(unreadMessageDtos, new SpecParams
            {
                PageIndex = page,
                PageSize = pageSize
            }, dto => dto);

            return (unreadCount: unreadMessagesList.Count, unreadMessages: paginated);
        }


        public async Task<bool> IsUserInConversationAsync(int conversationId, int userId)
        {
            return await _context.ConversationUsers
                .AnyAsync(cu => cu.ConversationId == conversationId && cu.UserId == userId);
        }

        public async Task<ConversationUser> JoinConversationAsync(int conversationId, int userId)
        {
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




        public async Task<PaginationResponse<ConversationDto>> GetUserConversationsAsync(int userId, int page = 1, int pageSize = 10)
        {
            var specParams = new SpecParams
            {
                PageIndex = page,
                PageSize = pageSize
            };


            var allConversations = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.ReadByUsers)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.ReplyToMessage)
                        .ThenInclude(rm => rm.Sender)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var paginatedResult = _paginationService.Paginate(allConversations, specParams, conversation =>
            {
                var dto = _mapper.Map<ConversationDto>(conversation);

                if (conversation.IsGroup)
                {
                    dto.Title = conversation.Title;
                    dto.GroupPictureFile = conversation.GroupPicture;
                }
                else
                {
                    dto.Title = GetOneToOneConversationTitle(conversation, userId);
                    // For private conversations, get the other person's profile picture
                    var otherUser = conversation.Participants
                        .FirstOrDefault(p => p.UserId != userId)?.User;
                    dto.GroupPictureFile = otherUser?.ProfilePicture;
                }

                dto.UnreadCount = conversation.Messages
                    .Count(m => !m.ReadByUsers.Any(r => r.UserId == userId) && m.SenderId != userId);

                if (dto.LastMessage != null)
                {
                    var lastMessage = conversation.Messages.FirstOrDefault();
                    dto.LastMessage.IsRead = lastMessage.ReadByUsers.Any(r => r.UserId == userId) ||
                                             lastMessage.SenderId == userId;
                }

                return dto;
            });

            return paginatedResult;
        }


        public async Task<ConversationDto> GetConversationByIdAsync(int conversationId, int userId)
        {
            // First check if user is part of this conversation
            bool isUserInConversation = await IsUserInConversationAsync(conversationId, userId);
            if (!isUserInConversation)
            {
                return null;
            }

            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.ReadByUsers)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.ReplyToMessage)
                        .ThenInclude(rm => rm.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
            {
                return null;
            }

            var dto = _mapper.Map<ConversationDto>(conversation);

            // Set correct title and picture based on group status
            if (conversation.IsGroup)
            {
                dto.Title = conversation.Title;
                dto.GroupPictureFile = conversation.GroupPicture;
            }
            else
            {
                dto.Title = GetOneToOneConversationTitle(conversation, userId);
                // For private conversations, get the other person's profile picture
                var otherUser = conversation.Participants
                    .FirstOrDefault(p => p.UserId != userId)?.User;
                dto.GroupPictureFile = otherUser?.ProfilePicture;
            }

            dto.UnreadCount = conversation.Messages
                .Count(m => !m.ReadByUsers.Any(r => r.UserId == userId) && m.SenderId != userId);

            if (dto.LastMessage != null)
            {
                var lastMessage = conversation.Messages.FirstOrDefault();
                dto.LastMessage.IsRead = lastMessage.ReadByUsers.Any(r => r.UserId == userId) ||
                                        lastMessage.SenderId == userId;
            }

            return dto;
        }


        public async Task<Conversation> CreateConversationAsync(CreateConversationDto dto, int creatorId)
        {
            /// Auto-detect if it should be a group based on participant count
            var totalParticipants = dto.ParticipantIds.Count + 1; // +1 for creator
            bool shouldBeGroup = totalParticipants > 2 || dto.IsGroup;

            // Check if it's a direct message and if a conversation already exists
            if (!shouldBeGroup && dto.ParticipantIds.Count == 1)
            {
                int otherUserId = dto.ParticipantIds[0];
                var existingConversation = await FindExistingDirectConversation(creatorId, otherUserId);
                if (existingConversation != null)
                {
                    return existingConversation;
                }
            }

            string conversationTitle;
            string conversationPicture = null;

            if (shouldBeGroup)
            {
                conversationTitle = !string.IsNullOrWhiteSpace(dto.Title) ? dto.Title : "Group Chat";


                if (dto.GroupPictureFile != null)
                {
                    conversationPicture = FilesSetting.UploadFile(dto.GroupPictureFile, "GroupPictures");
                }
            }
            else
            {
                // For private conversations: get other person's name and profile picture
                int otherUserId = dto.ParticipantIds[0];
                var otherUser = await _context.Users.FindAsync(otherUserId);

                if (otherUser != null)
                {
                    conversationTitle = $"{otherUser.FirstName} {otherUser.LastName}".Trim();
                    conversationPicture = otherUser.ProfilePicture;
                }
                else
                {
                    conversationTitle = "Private Chat";
                    conversationPicture = null;
                }
            }


            var conversation = new Conversation
            {
                Title = conversationTitle,
                IsGroup = shouldBeGroup, // Auto-detect based on participant count
                GroupPicture = conversationPicture,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Add all participants (including creator)
            var allParticipantIds = dto.ParticipantIds.Append(creatorId).Distinct().ToList();
            await AddParticipantsToConversation(conversation.Id, allParticipantIds);

            // Reload the conversation with all includes to ensure proper data
            var createdConversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversation.Id);

            return createdConversation ?? conversation;
        }


        #region Private Helper Methods

        private string GetOneToOneConversationTitle(Conversation conversation, int currentUserId)
        {
            var otherUser = conversation.Participants
                .FirstOrDefault(p => p.UserId != currentUserId)?.User;

            if (otherUser == null)
                return "Chat";

            return $"{otherUser.FirstName} {otherUser.LastName}".Trim();
        }

        private async Task<Conversation> FindExistingDirectConversation(int userId1, int userId2)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .Where(c => !c.IsGroup)
                .Where(c => c.Participants.Count() == 2)
                .Where(c => c.Participants.Any(p => p.UserId == userId1))
                .Where(c => c.Participants.Any(p => p.UserId == userId2))
                .FirstOrDefaultAsync();
        }

        private async Task AddParticipantsToConversation(int conversationId, IEnumerable<int> participantIds)
        {
            foreach (var userId in participantIds)
            {
                // Avoid tracking conflicts by checking first
                var existing = await _context.ConversationUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ConversationId == conversationId);

                if (existing == null)
                {
                    _context.ConversationUsers.Add(new ConversationUser
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateConversationTimestamp(int conversationId)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.UpdatedAt = DateTime.UtcNow;
            }
        }

        #endregion
    }
}