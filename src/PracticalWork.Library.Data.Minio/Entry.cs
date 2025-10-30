using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace PracticalWork.Library.Data.Minio;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для хранилища документов
    /// </summary>
    public static IServiceCollection AddMinioFileStorage(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddMinio( configuration["App:Minio:AccessKey"], configuration["App:Minio:SecretKey"]);
        serviceCollection.AddScoped<IObjectStorage, ObjectStorage>();
        
        return serviceCollection;
    }
}
