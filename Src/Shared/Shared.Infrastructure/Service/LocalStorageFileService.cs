using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.Application.Configuration;
using Shared.Domain.Constants;

namespace Shared.Infrastructure.Services;

public class LocalStorageFileService : IFileService
{
    private readonly AppConfig _appConfig;
    private readonly ICurrentUserService _currentUserService;
    private const string PUBLIC_FOLDER = "Public";
    public LocalStorageFileService(IOptions<AppConfig> appOptions, ICurrentUserService currentUserService)
    {
        _appConfig = appOptions.Value;
        _currentUserService = currentUserService;

    }

    public async Task<Response<string>> DeleteFile(string fileId)
    {
        try
        {
            if (HasAccess(fileId))
            {
                await Task.Delay(0);
                string path = Path.Combine(_appConfig.FileUploadTarget, fileId);
                File.Delete(path);
                return Response<string>.SuccessResponse("", Messages.DeletedSuccessfully);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
        catch (Exception ex)
        {
            return Response<string>.FailureResponse(Errors.Exception(ex));
        }
    }

    public async Task<byte[]> GetFile(string fileName)
    {
        try
        {
            if (HasAccess(fileName))
            {
                string path = Path.Combine(_appConfig.FileUploadTarget, fileName);
                return await File.ReadAllBytesAsync(path);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
        catch (FileNotFoundException)
        {
            throw new Exception("File already deleted.");
        }
        catch
        {
            throw;
        }
    }

    public async Task<Response<string>> ReplaceFile(IFormFile file, string oldName)
    {
        try
        {
            if (HasAccess(oldName))
            {
                string fileName = Path.GetFileNameWithoutExtension(oldName) + Path.GetExtension(file.FileName);
                File.Delete(Path.Combine(_appConfig.FileUploadTarget, oldName));
                string savedFileName = await SaveFile(file, fileName);
                return Response<string>.SuccessResponse(savedFileName, Messages.UploadedSuccessfully);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
        catch (Exception ex)
        {
            return Response<string>.FailureResponse(Errors.Exception(ex));
        }
    }

    public async Task<Response<string>> UploadFile(IFormFile file)
    {
        try
        {
            string userId = _currentUserService.UserName ?? throw new UnauthorizedAccessException();
            _ = Directory.CreateDirectory(_appConfig.FileUploadTarget);
            string fileName = userId + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName);
            string savedFileName = await SaveFile(file, fileName);
            return Response<string>.SuccessResponse(savedFileName, Messages.UploadedSuccessfully);
        }
        catch (Exception ex)
        {
            return Response<string>.FailureResponse(Errors.Exception(ex));
        }
    }

    private async Task<string> SaveFile(IFormFile file, string fileName, bool isPublic = false)
    {
        string filePath;
        if (isPublic)
        {
            filePath = Path.Combine(_appConfig.FileUploadTarget, PUBLIC_FOLDER, fileName);
        }
        else
        {
            filePath = Path.Combine(_appConfig.FileUploadTarget, fileName);
        }
        using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }
        return fileName;
    }

    private bool HasAccess(string fileName)
    {
        string fileOwnerId = fileName.Split("_")[0];
        return fileOwnerId == _currentUserService.UserName;
    }

    public async Task<byte[]> GetFileForOffice(string fileName)
    {
        try
        {
            //if (HasAccess(fileName))  TODO: find way to authorize office users but not client users
            //{
            string path = Path.Combine(_appConfig.FileUploadTarget, fileName);
            return await File.ReadAllBytesAsync(path);
            //}
            //else
            //{
            //    throw new UnauthorizedAccessException();
            //}
        }
        catch (FileNotFoundException)
        {
            throw new Exception("File already deleted.");
        }
        catch
        {
            throw;
        }
    }

    public async Task<byte[]> GetFilePublic(string fileName)
    {
        try
        {
            string path = Path.Combine(_appConfig.FileUploadTarget, PUBLIC_FOLDER, fileName);
            return await File.ReadAllBytesAsync(path);
        }
        catch (FileNotFoundException)
        {
            throw new Exception("File already deleted.");
        }
        catch (IOException)
        {
            throw new Exception("Invalid File");
        }
        catch
        {
            throw;
        }
    }

    public async Task<Response<string>> ReplaceFilePublic(IFormFile file, string oldName)
    {
        try
        {
            string fileName = Path.GetFileNameWithoutExtension(oldName) + Path.GetExtension(file.FileName);
            File.Delete(Path.Combine(_appConfig.FileUploadTarget, PUBLIC_FOLDER, oldName));
            string savedFileName = await SaveFile(file, fileName, true);
            return Response<string>.SuccessResponse(savedFileName, Messages.UploadedSuccessfully);
        }
        catch (Exception ex)
        {
            return Response<string>.FailureResponse(Errors.Exception(ex));
        }
    }

    public async Task<Response<string>> UploadFilePublic(IFormFile file)
    {
        try
        {
            _ = Directory.CreateDirectory(Path.Combine(_appConfig.FileUploadTarget, PUBLIC_FOLDER));
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string savedFileName = await SaveFile(file, fileName, true);
            return Response<string>.SuccessResponse(savedFileName, Messages.UploadedSuccessfully);
        }
        catch (Exception ex)
        {
            return Response<string>.FailureResponse(Errors.Exception(ex));
        }
    }
}
