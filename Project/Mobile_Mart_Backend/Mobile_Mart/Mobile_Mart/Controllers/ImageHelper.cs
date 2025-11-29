using Microsoft.AspNetCore.Http;

namespace Mobile_Mart.Controllers
{
    public static class ImageHelper
    {
        private static readonly string directory = "Images";

        #region SAVE FILE ASYNC
        public static async Task<string> SaveFileAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return string.Empty;

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", directory);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // ✅ Always store with leading slash
            return "/" + Path.Combine(directory, fileName).Replace("\\", "/");
        }

        #endregion

        #region delete file async
        public static async Task<string> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "Invalid file path.";

            string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);

            if (!System.IO.File.Exists(physicalPath))
                return "File not found.";

            try
            {
                await Task.Run(() => System.IO.File.Delete(physicalPath));
                return "File deleted successfully.";
            }
            catch (Exception ex)
            {
                return $"Error deleting file: {ex.Message}";
            }
        }
        #endregion


    }
}
