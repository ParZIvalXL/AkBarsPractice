#nullable enable
using PracticalWork.Library.Contracts.v1.Enums;
using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Library.Contracts.v2.Abstracts;

/// <summary>
/// Событие создания книги
/// </summary>
public sealed record BookCreatedEvent(
    Guid BookId,
    string Title,
    string Category,
    string[] Authors,
    int Year,
    DateTime CreatedAt
) : BaseLibraryEvent("book.created");