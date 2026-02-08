using PracticalWork.Library.Contracts.v2.Abstracts;
using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Library.Contracts.v2.Readers.Events;

/// <summary>
/// Событие закрытия карточки читателя
/// </summary>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="FullName">Полное имя читателя</param>
/// <param name="ClosedAt">Время закрытия</param>
/// <param name="Reason">Причина закрытия</param>
public sealed record ReaderClosedEvent(
    Guid ReaderId,
    string FullName,
    DateTime ClosedAt,
    string Reason
) : BaseLibraryEvent("reader.closed");
