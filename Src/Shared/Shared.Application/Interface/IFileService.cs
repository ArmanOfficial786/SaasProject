using Microsoft.AspNetCore.Http;

namespace Shared.Application.Interfaces;

public interface IFileService
{
    Task<Response<string>> DeleteFile(string fileId);
    Task<byte[]> GetFile(string fileName);
    Task<byte[]> GetFileForOffice(string fileName);
    Task<Response<string>> ReplaceFile(IFormFile file, string oldName);
    Task<Response<string>> UploadFile(IFormFile file);

    Task<byte[]> GetFilePublic(string fileName);
    Task<Response<string>> ReplaceFilePublic(IFormFile file, string oldName);
    Task<Response<string>> UploadFilePublic(IFormFile file);
}
