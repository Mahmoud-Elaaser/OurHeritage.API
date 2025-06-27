namespace OurHeritage.Service.DTOs.ChatDto
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsGroup { get; set; }
        public string? GroupPictureFile { get; set; }
        public List<UserPreviewDto> Participants { get; set; }
        public MessageDto LastMessage { get; set; }
        public int UnreadCount { get; set; } // Number of unread messages for current user
    }
}
