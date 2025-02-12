using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurHeritage.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<HandiCraft> HandiCrafts { get; set; }
        public ICollection<CulturalArticle> CulturalArticles { get; set; }
    }
}
