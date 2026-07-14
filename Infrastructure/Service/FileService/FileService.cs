
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Infrastructure.Service.FileService
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }
        public bool DeleteLocalFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }
            var path = Path.Combine(webHostEnvironment.WebRootPath, "Images", fileName);
            if (!File.Exists(path))
            {
                return false;
            }
            File.Delete(path);
            return true;

        }

        public async Task<string> SaveFileToLocalAsync(IFormFile file)
        {
            // Check if the Directory not exists
            var Dir = Path.Combine(webHostEnvironment.WebRootPath, "Images");
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            string? fileName = null;
            if (file != null)
            {
                // Creating the full Image Path
                fileName = Guid.NewGuid() + "_" + file.FileName;
                var Fullpath = Path.Combine(webHostEnvironment.WebRootPath, "Images", fileName);

                using (var FileStream = new FileStream(Fullpath, FileMode.Create))
                {
                    await file.CopyToAsync(FileStream);
                }

            }
            return fileName;
        }

        // Delete the Old File and add new one
        public async Task<string> UpdateFileToLocalAsync(IFormFile file, string OldfileName)
        {
            if (file == null)
            {
                return null;
            }

            // Delete the old File
            DeleteLocalFile(OldfileName);

            // Save and return the file
            var save = await SaveFileToLocalAsync(file);
            return save;
        }
    }
}
