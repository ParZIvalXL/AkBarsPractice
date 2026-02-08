using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using PracticalWork.Library.Contracts.v2.Messaging;

public sealed class RabbitMessagePublisher : IMessagePublisher
{
    private readonly IConnection _connection;

    public RabbitMessagePublisher(IConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: "library.events",
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event));

        await channel.BasicPublishAsync(
            exchange: "library.events",
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken);
    }
}