﻿using System.ComponentModel.DataAnnotations.Schema;

namespace OurHeritage.Core.Entities
{
    public class CulturalArticle
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Content { get; set; }
        public List<string>? ImageURL { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
