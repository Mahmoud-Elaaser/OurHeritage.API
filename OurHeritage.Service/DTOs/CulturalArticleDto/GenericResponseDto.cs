namespace OurHeritage.Service.DTOs.CulturalArticleDto
{
    public class GenericResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
