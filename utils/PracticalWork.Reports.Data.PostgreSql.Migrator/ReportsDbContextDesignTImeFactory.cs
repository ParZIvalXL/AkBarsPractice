using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PracticalWork.Reports.Data.PostgreSql.Migrator;

public sealed class ReportsDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<ReportsDbContext>
{
    public ReportsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();

        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseNpgsql(
                configuration.GetConnectionString("ReportsConnection")
            )
            .Options;

        return new ReportsDbContext(options);
    }
}