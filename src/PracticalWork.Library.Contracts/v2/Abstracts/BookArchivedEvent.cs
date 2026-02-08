using PracticalWork.Library.Contracts.v2.Abstracts;

/// <summary>
/// Событие создания новой книги в библиотеке
/// </summary>
public sealed record BookArchivedEvent(
    Guid BookId,
    string Title,
    string Reason,
    DateTime ArchivedAt
) : BaseLibraryEvent("book.archived");