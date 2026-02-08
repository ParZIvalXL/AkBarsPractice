using Microsoft.EntityFrameworkCore;
using PracticalWork.Reports.Data.PostgreSql.Entities;
using PracticalWork.Reports.Domain.Entities;

namespace PracticalWork.Reports.Data.PostgreSql;

public sealed class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options)
        : base(options) { }

    public DbSet<ActivityLogEntity> ActivityLogs => Set<ActivityLogEntity>();
    public DbSet<ReportMetadataEntity> ReportsMetadata => Set<ReportMetadataEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}