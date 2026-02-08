using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v2.DTOs;
using PracticalWork.Reports.Data.PostgreSql.Entities;
using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Reports.Data.PostgreSql.Repositories;

public sealed class ReportsRepository : IReportsRepository
{
    private readonly ReportsDbContext _db;

    public ReportsRepository(ReportsDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Запись активности
    /// </summary>
    /// <param name="log">Активность</param>
    public async Task InsertActivityLogAsync(ActivityLogDto log)
    {
        var entity = new ActivityLogEntity
        {
            Id = Guid.NewGuid(),
            EventType = log.EventType,
            OccurredOn = log.OccurredOn,
            Source = log.Source,
            Metadata = log.Metadata
        };

        _db.ActivityLogs.Add(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Получение логов
    /// </summary>
    /// <param name="from">С какого времени</param>
    /// <param name="to">По какое время</param>
    /// <param name="eventTypes">Фильтр событий</param>
    /// <returns>Логи активности</returns>
    public async Task<List<ActivityLogDto>> GetActivityLogsAsync(
        DateTime? from,
        DateTime? to,
        string[]? eventTypes)
    {
        IQueryable<ActivityLogEntity> query = _db.ActivityLogs;

        query = ApplyFilters(query, from, to, eventTypes);

        return await query
            .OrderByDescending(x => x.OccurredOn)
            .Select(x => new ActivityLogDto
            {
                Id = x.Id,
                EventType = x.EventType,
                OccurredOn = x.OccurredOn,
                Source = x.Source,
                Metadata = x.Metadata
            })
            .ToListAsync();
    }

    /// <summary>
    /// Получение логов с пагинацей
    /// </summary>
    /// <param name="from">С какого времени</param>
    /// <param name="to">По какое время</param>
    /// <param name="eventTypes">Фильтр событий</param>
    /// <param name="page">Страница</param>
    /// <param name="pageSize">Количество логов на странице</param>
    /// <returns>Логи</returns>
    public async Task<List<ActivityLogDto>> GetActivityLogsPagedAsync(
        DateTime? from,
        DateTime? to,
        string[]? eventTypes,
        int page,
        int pageSize)
    {
        IQueryable<ActivityLogEntity> query = _db.ActivityLogs;

        query = ApplyFilters(query, from, to, eventTypes);

        return await query
            .OrderByDescending(x => x.OccurredOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ActivityLogDto
            {
                Id = x.Id,
                EventType = x.EventType,
                OccurredOn = x.OccurredOn,
                Source = x.Source,
                Metadata = x.Metadata
            })
            .ToListAsync();
    }

    /// <summary>
    /// Сохранение метаданных для отчета
    /// </summary>
    /// <param name="metadata">Метаданные</param>
    public async Task InsertReportMetadataAsync(ReportMetadataDto metadata)
    {
        var entity = new ReportMetadataEntity
        {
            Id = Guid.NewGuid(),
            FileName = metadata.FileName,
            FileUrl = metadata.FileUrl,
            SizeBytes = metadata.SizeBytes,
            CreatedAt = metadata.CreatedAt
        };

        _db.ReportsMetadata.Add(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Получить метаданные
    /// </summary>
    /// <returns>Метаданные</returns>
    public async Task<List<ReportMetadataDto>> GetReportsMetadataAsync()
    {
        return await _db.ReportsMetadata
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReportMetadataDto
            {
                FileName = x.FileName,
                FileUrl = x.FileUrl,
                SizeBytes = x.SizeBytes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Метод для фильтрации
    /// </summary>
    /// <param name="query">Запрос</param>
    /// <param name="from">От</param>
    /// <param name="to">До</param>
    /// <param name="eventTypes">Фильтр по типу событий</param>
    /// <returns>Данные</returns>
    private static IQueryable<ActivityLogEntity> ApplyFilters(
        IQueryable<ActivityLogEntity> query,
        DateTime? from,
        DateTime? to,
        string[]? eventTypes)
    {
        if (from.HasValue)
            query = query.Where(x => x.OccurredOn >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.OccurredOn <= to.Value);

        if (eventTypes is { Length: > 0 })
        {
            var parsedTypes = eventTypes
                .Select(t => Enum.TryParse<ActivityEventType>(t, true, out var parsed)
                    ? parsed
                    : ActivityEventType.Unknown)
                .ToArray();

            query = query.Where(x => parsedTypes.Contains(x.EventType));
        }

        return query;
    }
}
