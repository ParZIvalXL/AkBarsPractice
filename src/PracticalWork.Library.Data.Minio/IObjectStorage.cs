using Minio.DataModel.Response;

namespace PracticalWork.Library.Data.Minio;

public interface IObjectStorage
{
    Task<PutObjectResponse> UploadFileAsync(string objectName, Stream fileStream);
    Task<Stream> DownloadFileStreamAsync(string objectName);
    Task DownloadFileToPathAsync(string objectName, string filePath);
    Task<string> GetFileUrlAsync(string objectName, int expirySeconds = 3600);
    Task DeleteFileAsync(string objectName);
    Task<bool> FileExistsAsync(string objectName);
}