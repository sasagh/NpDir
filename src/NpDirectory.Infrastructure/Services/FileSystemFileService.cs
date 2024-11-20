using Microsoft.AspNetCore.Http;
using NpDirectory.Application.Services;

namespace NpDirectory.Infrastructure.Services;

public class FileSystemFileService : IFileService
{
    
    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine("uploads", fileName);
        
        if (!Directory.Exists("uploads"))
        {
            Directory.CreateDirectory("uploads");
        }

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }
}