using Microsoft.AspNetCore.Http;

namespace Infrastructure.Service.FileService
{
    public interface IFileService
    {
        public Task<string> SaveFileToLocalAsync(IFormFile file);
        public bool DeleteLocalFile(string fileName);
        public Task<string> UpdateFileToLocalAsync(IFormFile file, string OldfileName);
    }
}
