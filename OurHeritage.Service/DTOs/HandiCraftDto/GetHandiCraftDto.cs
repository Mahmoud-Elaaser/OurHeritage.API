namespace OurHeritage.Service.DTOs.HandiCraftDto
{
    public class GetHandiCraftDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageOrVideo { get; set; } // Keeping as a List<string>
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string NameOfUser { get; set; }
        public string UserProfilePicture { get; set; }
    }

}
