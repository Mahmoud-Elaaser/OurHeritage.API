using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs.ChatDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IChatService
    {

        Task<PaginationResponse<ConversationDto>> GetUserConversationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<ConversationDto> GetConversationByIdAsync(int conversationId, int userId);
        Task<Conversation> CreateConversationAsync(CreateConversationDto dto, int creatorId);
        Task<Message> SendMessageAsync(SendMessageDto dto, int senderId);
        Task<Message> ReplyToMessageAsync(ReplyMessageDto dto, int senderId);
        Task<PaginationResponse<MessageDto>> GetConversationMessagesAsync(int conversationId, int userId, int page = 1, int pageSize = 20);
        Task<PaginationResponse<MessageDto>> GetAllMessagesAsync(int userId, int page = 1, int pageSize = 20);
        Task MarkMessageAsReadAsync(int messageId, int userId);
        Task MarkAllMessagesAsReadAsync(int conversationId, int userId);
        Task<(int unreadCount, PaginationResponse<MessageDto> unreadMessages)> GetUnreadMessagesAsync(int userId, int page = 1, int pageSize = 20);

        Task<bool> IsUserInConversationAsync(int conversationId, int userId);
        Task<ConversationUser> JoinConversationAsync(int conversationId, int userId);
    }
}