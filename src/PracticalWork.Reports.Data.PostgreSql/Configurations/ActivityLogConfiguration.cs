using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PracticalWork.Reports.Domain.Entities;

namespace PracticalWork.Reports.Data.PostgreSql.Configurations;

public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.EventType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.EventDate)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.ExternalBookId)
            .HasColumnType("uuid");

        builder.Property(x => x.ExternalReaderId)
            .HasColumnType("uuid");

        builder.Property(x => x.Metadata)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.EventDate);
        builder.HasIndex(x => x.EventType);
    }
}