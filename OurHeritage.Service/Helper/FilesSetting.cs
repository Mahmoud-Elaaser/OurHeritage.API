using Microsoft.AspNetCore.Http;

namespace OurHeritage.Service.Helper
{
    public static class FilesSetting
    {
        public static string UploadFile(IFormFile file, string FolderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not selected or is empty.");
            }

            // 1. Get Located Folder Path 
            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\File", FolderName);

            // 2. Get File Name and Make it Unique 
            string FileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";  // Use Path.GetExtension to extract the file extension

            // 3. Get File Path[Folder Path + FileName]
            string FilePath = Path.Combine(FolderPath, FileName);

            // 4. Ensure the folder exists
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            // 5. Save File As Streams
            using var Fs = new FileStream(FilePath, FileMode.Create);
            file.CopyTo(Fs);

            // 6. Return File Name
            return FileName;
        }




        //Delete
        public static void DeleteFile(string FileName, string FolderName)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                throw new ArgumentException("File name is not provided.");
            }

            // 1. Get Located Folder Path 

            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\File", FolderName);

            // 2. Get File Path [Folder Path + FileName]
            string FilePath = Path.Combine(FolderPath, FileName);

            // 3. Check if the file exists
            if (File.Exists(FilePath))
            {
                // 4. Delete the file
                File.Delete(FilePath);
            }

        }
    }
}

