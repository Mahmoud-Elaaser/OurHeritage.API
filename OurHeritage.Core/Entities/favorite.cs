using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurHeritage.Core.Entities
{
   public class favorite
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int HandiCraftId { get; set; }
        public HandiCraft HandiCraft { get; set; }
    }
}
