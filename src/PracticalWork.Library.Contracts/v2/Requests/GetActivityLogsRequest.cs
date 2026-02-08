using JetBrains.Annotations;

namespace PracticalWork.Library.Contracts.v2.Requests;

public sealed class GetActivityLogsRequest
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    /// <summary>
    /// Типы событий (например: book.created, reader.closed)
    /// </summary>
    [CanBeNull]
    public string[] EventTypes { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}