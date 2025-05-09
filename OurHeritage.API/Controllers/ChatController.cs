using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Specifications;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResponse<ConversationDto>>> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = GetCurrentUserId();
            var paginatedConversations = await _chatService.GetUserConversationsAsync(userId, page, pageSize);
            return Ok(paginatedConversations);
        }


        [HttpPost("conversations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> CreateConversation(CreateConversationDto dto)
        {
            int userId = GetCurrentUserId();
            var conversation = await _chatService.CreateConversationAsync(dto, userId);
            return Ok(new { conversationId = conversation.Id });
        }


        [HttpPost("conversations/join")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> JoinConversation(JoinConversationDto dto)
        {
            int userId = GetCurrentUserId();
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConversationDto>> GetConversation(int id)
        {
            int userId = GetCurrentUserId();
            var conversation = await _chatService.GetConversationByIdAsync(id, userId);

            if (conversation == null)
            {
                return NotFound();
            }

            // Mark all messages as read
            await _chatService.MarkAllMessagesAsReadAsync(id, userId);

            // Reset unread count as we've marked everything as read
            conversation.UnreadCount = 0;

            return Ok(conversation);
        }


        [HttpGet("conversations/{id}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaginationResponse<MessageDto>>> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = GetCurrentUserId();
            var messages = await _chatService.GetConversationMessagesAsync(id, userId, page, pageSize);

            if (messages == null)
            {
                return NotFound();
            }

            return Ok(messages);
        }


        [HttpGet("messages/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResponse<MessageDto>>> GetAllMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = GetCurrentUserId();
            var paginatedMessages = await _chatService.GetAllMessagesAsync(userId, page, pageSize);
            return Ok(paginatedMessages);
        }



        [HttpPost("messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> SendMessage(SendMessageDto dto)
        {
            int userId = GetCurrentUserId();
            var message = await _chatService.SendMessageAsync(dto, userId);

            if (message == null)
            {
                return BadRequest("You are not part of this conversation.");
            }

            return Ok(new { messageId = message.Id });
        }


        [HttpPost("messages/reply")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> ReplyToMessage(ReplyMessageDto dto)
        {
            int userId = GetCurrentUserId();
            var message = await _chatService.ReplyToMessageAsync(dto, userId);

            if (message == null)
            {
                return BadRequest("You are not part of this conversation or the referenced message is invalid.");
            }

            return Ok(new { messageId = message.Id });
        }


        [HttpPost("messages/{id}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> MarkAsRead(int id)
        {
            int userId = GetCurrentUserId();
            await _chatService.MarkMessageAsReadAsync(id, userId);

            return Ok("Message has been marked as read");
        }


        [HttpGet("unread")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetUnreadMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = GetCurrentUserId();
            var (unreadCount, paginatedUnreadMessages) = await _chatService.GetUnreadMessagesAsync(userId, page, pageSize);

            return Ok(new
            {
                unreadCount,
                unreadMessages = paginatedUnreadMessages
            });
        }


        // Helper method to extract user ID from claims
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}