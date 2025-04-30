namespace OurHeritage.Service.DTOs.OrderDto
{
    public class HandiCraftResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public List<string> ImageOrVideo { get; set; }
    }
}
