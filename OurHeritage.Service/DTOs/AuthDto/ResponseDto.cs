namespace OurHeritage.Service.DTOs.AuthDto
{
    public class ResponseDto
    {
        public bool IsSucceeded { get; set; }
        public int Status { get; set; }
        public string? Message { get; set; }
        public object? Model { get; set; }
        public IEnumerable<object>? Models { get; set; }
    }
}
