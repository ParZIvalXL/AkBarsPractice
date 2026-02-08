#nullable enable
using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Library.Contracts.v2.DTOs;

public sealed class ActivityLogDto
{
    public Guid Id { get; init; }
    public ActivityEventType EventType { get; init; }
    public DateTime OccurredOn { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? Metadata { get; init; } = string.Empty;
}