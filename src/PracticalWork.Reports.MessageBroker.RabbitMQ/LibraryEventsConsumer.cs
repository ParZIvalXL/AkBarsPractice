using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v2.DTOs;
using PracticalWork.Reports.Data.PostgreSql;
using PracticalWork.Reports.Domain.Entities;
using PracticalWork.Reports.Domain.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PracticalWork.Reports.MessageBroker.RabbitMQ;

public sealed class LibraryEventsConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LibraryEventsConsumer> _logger;
    private IChannel? _channel;

    public LibraryEventsConsumer(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<LibraryEventsConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: "library.events",
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        var queue = await _channel.QueueDeclareAsync(
            queue: "reports.activity",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: "library.events",
            routingKey: string.Empty);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("LibraryEventsConsumer started and listening to RabbitMQ.");
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var eventType = ExtractEventType(json);

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider
                .GetRequiredService<IReportsRepository>();

            await repository.InsertActivityLogAsync(new ActivityLogDto
            {
                EventType = eventType,
                OccurredOn = DateTime.UtcNow,
                Source = "library",
                Metadata = json
            });

            if (_channel is { IsOpen: true })
                await _channel.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process library event");

            if (_channel is { IsOpen: true })
                await _channel.BasicNackAsync(args.DeliveryTag, false, true);
        }
    }
    

    private static ActivityEventType ExtractEventType(string json)
    {
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("EventType", out var prop))
            return ActivityEventType.Unknown;

        return prop.ValueKind switch
        {
            JsonValueKind.Number => (ActivityEventType)prop.GetInt32(),
            JsonValueKind.String => prop.GetString() switch
            {
                "Created" => ActivityEventType.Created,
                "Updated" => ActivityEventType.Updated,
                "Deleted" => ActivityEventType.Deleted,
                _ => ActivityEventType.Unknown
            },
            _ => ActivityEventType.Unknown
        };
    }

    public override void Dispose()
    {
        if (_channel != null && _channel.IsOpen)
        {
            _channel.CloseAsync().GetAwaiter().GetResult();
        }
        _channel?.Dispose();
        base.Dispose();
    }
}