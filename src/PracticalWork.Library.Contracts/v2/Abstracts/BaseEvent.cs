namespace PracticalWork.Library.Contracts.v2.Abstracts;

/// <summary>
/// Базовый рекорд для всех событий в системе
/// </summary>
/// <param name="EventId">Уникальный идентификатор события</param>
/// <param name="OccurredOn">Дата и время возникновения события в UTC</param>
/// <param name="EventType">Тип события</param>
/// <param name="Source">Источник события</param>
public abstract record BaseEvent(
    Guid EventId,
    DateTime OccurredOn,
    string EventType,
    string Source
)
{
    protected BaseEvent(string eventType, string source)
        : this(Guid.NewGuid(), DateTime.UtcNow, eventType, source)
    {
    }
}