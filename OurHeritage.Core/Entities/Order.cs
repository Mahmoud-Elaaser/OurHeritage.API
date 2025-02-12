using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurHeritage.Core.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public double TotalPrice { get; set; }
        public ICollection<HandiCraft> HandiCrafts { get; set; } = new List<HandiCraft>();
    }
}
