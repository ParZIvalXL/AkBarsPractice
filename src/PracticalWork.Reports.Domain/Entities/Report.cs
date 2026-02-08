namespace PracticalWork.Reports.Domain.Entities;

public sealed class Report
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public string FilePath { get; set; } = default!;

    public DateTime GeneratedAt { get; set; }

    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}