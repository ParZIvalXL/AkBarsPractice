using System.Globalization;
using CsvHelper;
using JetBrains.Annotations;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Contracts.v2.DTOs;
using PracticalWork.Library.Contracts.v2.Requests;
using PracticalWork.Library.Data.Minio;

namespace PracticalWork.Library.Services;

public class ReportsService : IReportsService
{
    private const string ReportsBucket = "reports";
    private const int ReportLifetimeSeconds = 60 * 60;

    private readonly IReportsRepository _repository;
    private readonly IObjectStorage _objectStorage;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _cacheKeyGenerator;

    public ReportsService(
        IReportsRepository repository,
        IObjectStorage objectStorage,
        ICacheService cache,
        ICacheKeyGenerator cacheKeyGenerator)
    {
        _repository = repository;
        _objectStorage = objectStorage;
        _cache = cache;
        _cacheKeyGenerator = cacheKeyGenerator;
    }

    public async Task LogActivityAsync(ActivityLogDto log)
    {
        if (log.Id == Guid.Empty)
            throw new ArgumentException("EventId обязателен");

        await _repository.InsertActivityLogAsync(log);
    }
    
    public async Task<ReportMetadataDto> GenerateReportAsync(GenerateReportRequest request)
    {
        var logs = await _repository.GetActivityLogsAsync(
            request.From,
            request.To,
            request.EventTypes
        );

        var fileName = $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        await using var stream = new MemoryStream();
        await using (var writer = new StreamWriter(stream, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(logs);
            await writer.FlushAsync();
        }

        stream.Position = 0;

        await _objectStorage.UploadFileAsync($"{ReportsBucket}/{fileName}", stream);

        var metadata = new ReportMetadataDto
        {
            FileName = fileName,
            FileUrl = string.Empty,
            CreatedAt = DateTime.UtcNow,
            SizeBytes = stream.Length
        };

        await _repository.InsertReportMetadataAsync(metadata);
        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.ReportsListPrefix);

        return metadata;
    }

    public async Task<List<ActivityLogDto>> GetActivityLogsAsync(
        DateTime? from,
        DateTime? to,
        [CanBeNull] string[] eventTypes,
        int page = 1,
        int pageSize = 20)
    {
        return await _repository.GetActivityLogsPagedAsync(
            from, to, eventTypes, page, pageSize);
    }

    public async Task<List<ReportMetadataDto>> GetReportsListAsync()
    {
        var cacheKey = _cacheKeyGenerator.ReportsListKey;

        var cached = await _cache.GetAsync<List<ReportMetadataDto>>(cacheKey);
        if (cached != null)
            return cached;

        var reports = await _repository.GetReportsMetadataAsync();

        await _cache.SetAsync(cacheKey, reports, TimeSpan.FromHours(24));

        return reports;
    }

    public async Task<string> GetReportDownloadUrlAsync(string reportName)
    {
        var path = $"{ReportsBucket}/{reportName}";

        if (!await _objectStorage.FileExistsAsync(path))
            throw new ArgumentException("Отчет не найден");

        return await _objectStorage.GetFileUrlAsync(path, ReportLifetimeSeconds);
    }
}
