namespace Scobius.Core.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName);
    Task DeleteFileAsync(string fileName, string bucketName);
}