using OurHeritage.Service.DTOs.FavoriteDto;

namespace OurHeritage.Service.DTOs.HandiCraftDto
{
    public class GetHandiCraftDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageOrVideo { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int UserId { get; set; }
        public string NameOfUser { get; set; }
        public string UserProfilePicture { get; set; }
        public string DateAdded { get; set; }
        public string TimeAgo { get; set; }
        public int FavoriteCount { get; set; }
        public List<GetFavoriteDto> FavoritedBy { get; set; }  //  List of users who favorited this HandiCraft
    }


}
