using Microsoft.AspNetCore.Http;

namespace NpDirectory.Application.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file);
}