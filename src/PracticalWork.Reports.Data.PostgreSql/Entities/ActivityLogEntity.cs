using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Reports.Data.PostgreSql.Entities;

public class ActivityLogEntity
{
    public Guid Id { get; set; }
    public ActivityEventType EventType { get; set; }
    public DateTime OccurredOn { get; set; }
    public string Source { get; set; } = null!;
    public string? Metadata { get; set; }
}