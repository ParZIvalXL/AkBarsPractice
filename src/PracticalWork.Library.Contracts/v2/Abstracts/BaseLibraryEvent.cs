using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Library.Contracts.v2.Abstracts;


/// <summary>
/// Базовый рекорд для всех событий библиотеки
/// </summary>
public abstract record BaseLibraryEvent : BaseEvent
{
    protected BaseLibraryEvent(string eventType)
        : base(eventType, "library-service")
    {
    }
}
