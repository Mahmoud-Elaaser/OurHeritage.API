namespace OurHeritage.Service.DTOs.HandiCraftDto
{
    public class GetHandiCraftDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageOrVideo { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
    }
}
