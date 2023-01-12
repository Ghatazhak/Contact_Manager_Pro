using Contact_Manager_Pro.Services.Interfaces;

namespace Contact_Manager_Pro.Services
{
    public class ImageService : IImageService
    {
        private readonly string[] _suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        private readonly string _defaultImage = "img/DefaultContactImage.png";

        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if (fileData is null) return _defaultImage;

            try
            {
                // Convert fileData into a binary string
                string imageBase64Data = Convert.ToBase64String(fileData);
                // format the string for the HTML img tag and return it.
                return string.Format($"data:{extension};base64,{imageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                // Have to use a memory stream for the file upload
                using MemoryStream memoryStream = new();
                // Copy the file into the memory stream.
                await file.CopyToAsync(memoryStream);
                // Once the file is in the memory stream convert it to array and return it.
                byte[] byteFile = memoryStream.ToArray();
                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
