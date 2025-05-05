using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Service.DTOs.ChatDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var conversations = await _chatService.GetUserConversationsAsync(userId);

            // Map to DTOs
            var conversationDtos = conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                Title = c.IsGroup ? c.Title : c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.FirstName + " " + c.Participants.FirstOrDefault(p => p.UserId != userId)?.User.LastName,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsGroup = c.IsGroup,
                Participants = c.Participants.Select(p => new UserPreviewDto
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    ProfilePicture = p.User.ProfilePicture
                }).ToList(),
                LastMessage = c.Messages.Any() ? new MessageDto
                {
                    Id = c.Messages.First().Id,
                    ConversationId = c.Messages.First().ConversationId,
                    Content = c.Messages.First().Content,
                    SentAt = c.Messages.First().SentAt,
                    Type = c.Messages.First().Type,
                    Attachment = c.Messages.First().Attachment,
                    Sender = new UserPreviewDto
                    {
                        Id = c.Messages.First().Sender.Id,
                        FirstName = c.Messages.First().Sender.FirstName,
                        LastName = c.Messages.First().Sender.LastName,
                        ProfilePicture = c.Messages.First().Sender.ProfilePicture
                    },
                    IsRead = c.Messages.First().ReadByUsers.Any(r => r.UserId == userId) || c.Messages.First().SenderId == userId
                } : null,
                UnreadCount = c.Messages.Count(m => !m.ReadByUsers.Any(r => r.UserId == userId) && m.SenderId != userId)
            }).ToList();

            return Ok(conversationDtos);
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation(CreateConversationDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var conversation = await _chatService.CreateConversationAsync(dto, userId);

            return Ok(new { conversationId = conversation.Id });
        }

        [HttpPost("conversations/join")]
        public async Task<IActionResult> JoinConversation(JoinConversationDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var conversationUser = await _chatService.JoinConversationAsync(dto.ConversationId, userId);

            if (conversationUser == null)
            {
                return NotFound("Conversation not found");
            }

            return Ok(new
            {
                conversationId = dto.ConversationId,
                joinedAt = conversationUser.JoinedAt
            });
        }

        [HttpGet("conversations/{id}")]
        public async Task<IActionResult> GetConversation(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var conversation = await _chatService.GetConversationByIdAsync(id, userId);

            if (conversation == null)
            {
                return NotFound();
            }

            // Mark all messages as read
            await _chatService.MarkAllMessagesAsReadAsync(id, userId);

            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Title = conversation.IsGroup ? conversation.Title : conversation.Participants.FirstOrDefault(p => p.UserId != userId)?.User.FirstName + " " + conversation.Participants.FirstOrDefault(p => p.UserId != userId)?.User.LastName,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                IsGroup = conversation.IsGroup,
                Participants = conversation.Participants.Select(p => new UserPreviewDto
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    ProfilePicture = p.User.ProfilePicture
                }).ToList(),
                UnreadCount = 0
            };

            return Ok(conversationDto);
        }



        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var messages = await _chatService.GetConversationMessagesAsync(id, userId, page, pageSize);

            if (messages == null)
            {
                return NotFound();
            }

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                Content = m.Content,
                SentAt = m.SentAt,
                Type = m.Type,
                Attachment = m.Attachment,
                Sender = new UserPreviewDto
                {
                    Id = m.Sender.Id,
                    FirstName = m.Sender.FirstName,
                    LastName = m.Sender.LastName,
                    ProfilePicture = m.Sender.ProfilePicture
                },
                IsRead = m.ReadByUsers.Any(r => r.UserId == userId) || m.SenderId == userId,
                ReadBy = m.ReadByUsers.Select(r => new UserPreviewDto
                {
                    Id = r.User.Id,
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName,
                    ProfilePicture = r.User.ProfilePicture
                }).ToList(),
                // Include reply information
                ReplyToMessageId = m.ReplyToMessageId,
                ReplyToMessage = m.ReplyToMessage != null ? new ReplyPreviewDto
                {
                    Id = m.ReplyToMessage.Id,
                    Content = m.ReplyToMessage.Content,
                    Type = m.ReplyToMessage.Type,
                    Sender = m.ReplyToMessage.Sender != null ? new UserPreviewDto
                    {
                        Id = m.ReplyToMessage.Sender.Id,
                        FirstName = m.ReplyToMessage.Sender.FirstName,
                        LastName = m.ReplyToMessage.Sender.LastName,
                        ProfilePicture = m.ReplyToMessage.Sender.ProfilePicture
                    } : null
                } : null
            }).ToList();

            return Ok(messageDtos);
        }
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var message = await _chatService.SendMessageAsync(dto, userId);

            if (message == null)
            {
                return BadRequest("You are not part of this conversation.");
            }

            return Ok(new { messageId = message.Id });
        }


        [HttpPost("messages/reply")]
        public async Task<IActionResult> ReplyToMessage(ReplyMessageDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var message = await _chatService.ReplyToMessageAsync(dto, userId);

            if (message == null)
            {
                return BadRequest("You are not part of this conversation or the referenced message is invalid.");
            }

            return Ok(new { messageId = message.Id });
        }

        [HttpPost("messages/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _chatService.MarkMessageAsReadAsync(id, userId);

            return Ok("Message has been marked as read");
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (unreadCount, unreadMessages) = await _chatService.GetUnreadMessagesAsync(userId);

            // Map to DTOs with ReadBy information and reply information
            var messageDtos = unreadMessages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                Content = m.Content,
                SentAt = m.SentAt,
                Type = m.Type,
                Attachment = m.Attachment,
                Sender = new UserPreviewDto
                {
                    Id = m.Sender.Id,
                    FirstName = m.Sender.FirstName,
                    LastName = m.Sender.LastName,
                    ProfilePicture = m.Sender.ProfilePicture
                },
                IsRead = false, // We know they're all unread for the current user
                ReadBy = m.ReadByUsers.Select(r => new UserPreviewDto // Get all users who have read this message
                {
                    Id = r.User.Id,
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName,
                    ProfilePicture = r.User.ProfilePicture
                }).ToList(),
                // Include reply information
                ReplyToMessageId = m.ReplyToMessageId,
                ReplyToMessage = m.ReplyToMessage != null ? new ReplyPreviewDto
                {
                    Id = m.ReplyToMessage.Id,
                    Content = m.ReplyToMessage.Content,
                    Type = m.ReplyToMessage.Type,
                    Sender = m.ReplyToMessage.Sender != null ? new UserPreviewDto
                    {
                        Id = m.ReplyToMessage.Sender.Id,
                        FirstName = m.ReplyToMessage.Sender.FirstName,
                        LastName = m.ReplyToMessage.Sender.LastName,
                        ProfilePicture = m.ReplyToMessage.Sender.ProfilePicture
                    } : null
                } : null
            }).ToList();

            return Ok(new
            {
                unreadCount,
                unreadMessages = messageDtos
            });
        }


    }
}
