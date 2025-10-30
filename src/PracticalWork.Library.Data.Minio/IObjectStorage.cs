using Minio.DataModel.Response;

namespace PracticalWork.Library.Data.Minio;

public interface IObjectStorage
{
    Task<PutObjectResponse> UploadFileAsync(string objectName, Stream fileStream);
    Task<string> DownloadFileAsync(string objectName);
    Task DeleteFileAsync(string objectName);
}