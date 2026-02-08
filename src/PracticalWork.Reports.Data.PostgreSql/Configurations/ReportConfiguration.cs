using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PracticalWork.Reports.Domain.Entities;

namespace PracticalWork.Reports.Data.PostgreSql.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.GeneratedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.PeriodFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.PeriodTo)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasColumnType("int");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.GeneratedAt);
    }
}