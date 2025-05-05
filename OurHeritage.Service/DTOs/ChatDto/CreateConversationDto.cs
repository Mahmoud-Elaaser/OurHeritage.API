using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurHeritage.Service.DTOs.ChatDto
{
    public class CreateConversationDto
    {
        public string Title { get; set; } 
        public bool IsGroup { get; set; } = false;
        public List<int> ParticipantIds { get; set; } 
    }
}
