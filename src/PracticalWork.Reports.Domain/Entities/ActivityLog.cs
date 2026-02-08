using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Reports.Domain.Entities;

public sealed class ActivityLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid? ExternalBookId { get; private set; }
    public Guid? ExternalReaderId { get; private set; }
    public ActivityEventType EventType { get; private set; }
    public DateTime EventDate { get; private set; }

    public string Metadata { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;

    private ActivityLog() { }

    public ActivityLog(
        ActivityEventType eventType,
        DateTime eventDate,
        string metadata,
        Guid? bookId = null,
        Guid? readerId = null)
    {
        EventType = eventType;
        EventDate = eventDate;
        Metadata = metadata;
        ExternalBookId = bookId;
        ExternalReaderId = readerId;
    }
}