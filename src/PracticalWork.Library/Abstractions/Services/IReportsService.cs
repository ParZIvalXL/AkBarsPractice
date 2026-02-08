using JetBrains.Annotations;
using PracticalWork.Library.Contracts.v2.DTOs;
using PracticalWork.Library.Contracts.v2.Requests;

namespace PracticalWork.Library.Abstractions.Services
{
    public interface IReportsService
    {
        Task LogActivityAsync(ActivityLogDto log);

        Task<ReportMetadataDto> GenerateReportAsync(GenerateReportRequest request);

        Task<List<ActivityLogDto>> GetActivityLogsAsync(DateTime? from, DateTime? to, [CanBeNull] string[] eventTypes, int page = 1, int pageSize = 20);

        Task<List<ReportMetadataDto>> GetReportsListAsync();

        Task<string> GetReportDownloadUrlAsync(string reportName);
    }
}