using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OurHeritage.Service.Helper
{
    public static class FilesSetting
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private static IConfiguration _configuration;

        public static void Configure(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public static string UploadFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not selected or is empty.");
            }

            // 1. Get Located Folder Path
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "File", folderName);

            // 2. Get File Name and Make it Unique 
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // 3. Get File Path [Folder Path + FileName]
            string filePath = Path.Combine(folderPath, fileName);

            // 4. Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 5. Save File As Streams
            using var fs = new FileStream(filePath, FileMode.Create);
            file.CopyTo(fs);

            // 6. Get Base URL
            string baseUrl = _configuration["ApiBaseUrl"] ?? $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            // 7. Generate and Return File URL
            return $"{baseUrl}/File/{folderName}/{fileName}";
        }

        public static void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("File URL is not provided.");
            }

            // Extract File Name from URL
            Uri uri = new Uri(fileUrl);
            string fileName = Path.GetFileName(uri.LocalPath);

            // Get Folder Path
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "File");

            // Search for the file in all folders inside "File"
            string[] allPossibleFiles = Directory.GetFiles(folderPath, fileName, SearchOption.AllDirectories);

            // If file exists, delete it
            foreach (var filePath in allPossibleFiles)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
