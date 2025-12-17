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
            .WithSSL(_configuration["App:Minio:UseSsl"]?.ToLower() == "true") // опционально
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

    public async Task<Stream> DownloadFileStreamAsync(string objectName)
    {
        var memoryStream = new MemoryStream();
        
        var args = new GetObjectArgs()
            .WithBucket(_configuration["App:Minio:BucketName"])
            .WithObject(objectName)
            .WithCallbackStream(async stream => 
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
            });
        
        await _minioClient.GetObjectAsync(args);
        return memoryStream;
    }

    public Task DownloadFileToPathAsync(string objectName, string filePath)
    {
        var args = new GetObjectArgs()
            .WithBucket(_configuration["App:Minio:BucketName"])
            .WithObject(objectName)
            .WithFile(filePath);
        
        return _minioClient.GetObjectAsync(args);
    }

    public async Task<string> GetFileUrlAsync(string objectName, int expirySeconds = 3600)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_configuration["App:Minio:BucketName"])
            .WithObject(objectName)
            .WithExpiry(expirySeconds);
        
        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public Task DeleteFileAsync(string objectName)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_configuration["App:Minio:BucketName"])
            .WithObject(objectName);
        
        return _minioClient.RemoveObjectAsync(args);
    }

    public async Task<bool> FileExistsAsync(string objectName)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_configuration["App:Minio:BucketName"])
                .WithObject(objectName);
            
            await _minioClient.StatObjectAsync(args);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}