using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.ChatDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IChatService
    {
        Task<List<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation> GetConversationByIdAsync(int conversationId, int userId);
        Task<Conversation> CreateConversationAsync(CreateConversationDto dto, int creatorId);
        Task<Message> SendMessageAsync(SendMessageDto dto, int senderId);
        Task<List<Message>> GetConversationMessagesAsync(int conversationId, int userId, int page = 1, int pageSize = 20);
        Task MarkMessageAsReadAsync(int messageId, int userId);
        Task MarkAllMessagesAsReadAsync(int conversationId, int userId);
        Task<(int Count, List<Message> Messages)> GetUnreadMessagesAsync(int userId);
        Task<bool> IsUserInConversationAsync(int conversationId, int userId);

        Task<Message> ReplyToMessageAsync(ReplyMessageDto dto, int senderId);
        Task<ConversationUser> JoinConversationAsync(int conversationId, int userId);
    }
}
