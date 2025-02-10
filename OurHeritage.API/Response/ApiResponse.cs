namespace OurHeritage.API.Response
{
    public class ApiResponse
    {
        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetStatusCodeMessage(statusCode);
        }
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        private string? GetStatusCodeMessage(int? statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Un Autorized",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => null
            };
        }
    }
}
