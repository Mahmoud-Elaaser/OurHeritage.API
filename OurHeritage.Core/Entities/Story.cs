using System.ComponentModel.DataAnnotations.Schema;

namespace OurHeritage.Core.Entities
{
    public class Story
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string Content { get; set; }
    }
}
