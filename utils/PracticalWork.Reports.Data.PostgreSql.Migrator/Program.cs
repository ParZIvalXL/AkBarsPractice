using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PracticalWork.Reports.Data.PostgreSql;

namespace PracticalWork.Reports.Data.PostgreSql.Migrator;

[UsedImplicitly]
public class Program
{
    private const string AppName = "PracticalWork.Reports.Data.PostgreSql.Migrator";

    private static IConfiguration Configuration { get; set; } = null!;

    private static readonly ILogger SystemLogger = CreateSystemLogger();

    public static async Task Main()
    {
        try
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            await MigrateDatabase();
        }
        catch (Exception exception)
        {
            SystemLogger.LogCritical(exception, "Critical error in Reports Migrator");
            throw;
        }
    }

    private static async Task MigrateDatabase()
    {
        var serviceProvider = CreateServices();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting migrations for Reports database");

        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();
        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Reports database migrated successfully");
    }

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();

        return services
            .AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddConsole();
            })
            .AddDbContext<ReportsDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("ReportsConnection"),
                    npgsql =>
                        npgsql.CommandTimeout(
                            Configuration.GetValue<int>("App:MigrationTimeoutInSeconds")
                        )
                )
            )
            .BuildServiceProvider(false);
    }

    private static ILogger CreateSystemLogger()
    {
        return new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            })
            .BuildServiceProvider()
            .GetRequiredService<ILogger<Program>>();
    }
}
