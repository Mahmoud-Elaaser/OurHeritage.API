using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurHeritage.Core.Entities
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CulturalArticleId { get; set; }
        public CulturalArticle CulturalArticle { get; set; }
        //public int? CommentId { get; set; }
        //public Comment Comment { get; set; }
    }
}
