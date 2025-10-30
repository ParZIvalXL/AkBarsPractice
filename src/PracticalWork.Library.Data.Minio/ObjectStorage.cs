using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;

namespace PracticalWork.Library.Data.Minio;

public class ObjectStorage : IObjectStorage
{
    private readonly MinioClient _minioClient;
    private readonly IConfiguration _configuration;

    public ObjectStorage(IConfiguration configuration)
    {
        _configuration = configuration;
        _minioClient = new MinioClient()
            .WithEndpoint(_configuration["App:Minio:Endpoint"])
            .WithCredentials(_configuration["App:Minio:AccessKey"], _configuration["App:Minio:SecretKey"])
            .Build() as MinioClient;
    }

    public Task<PutObjectResponse> UploadFileAsync(string objectName, Stream fileStream)
    {
        return _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_configuration["App:Minio:BucketName"])
            .WithObjectSize(fileStream.Length)
            .WithObject(objectName)
            .WithStreamData(fileStream));
    }

    public Task<string> DownloadFileAsync(string objectName)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFileAsync(string objectName)
    {
        throw new NotImplementedException();
    }
}