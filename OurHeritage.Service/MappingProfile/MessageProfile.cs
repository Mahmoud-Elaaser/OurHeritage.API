using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.ChatDto;

namespace OurHeritage.Service.MappingProfile
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<User, UserPreviewDto>();

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
                .ForMember(dest => dest.ReadBy, opt => opt.MapFrom(src => src.ReadByUsers.Select(r => r.User)))
                .ForMember(dest => dest.IsRead, opt => opt.Ignore()); // Will be set in service based on current user

            CreateMap<Message, ReplyPreviewDto>()
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender));

            CreateMap<Conversation, ConversationDto>()
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src =>
                    src.Participants.Select(p => p.User)))
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src =>
                    src.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()))
                .ForMember(dest => dest.Title, opt => opt.Ignore()) // Will be set in service based on group status
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());
        }
    }
}
