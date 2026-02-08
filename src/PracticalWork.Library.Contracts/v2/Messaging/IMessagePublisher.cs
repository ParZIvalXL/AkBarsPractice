namespace PracticalWork.Library.Contracts.v2.Messaging;

/// <summary>
/// Публикация доменных событий
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}