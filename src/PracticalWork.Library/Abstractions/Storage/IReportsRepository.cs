using JetBrains.Annotations;
using PracticalWork.Library.Contracts.v2.DTOs;
using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Library.Abstractions.Storage
{
    public interface IReportsRepository
    {
        Task InsertActivityLogAsync(ActivityLogDto log);

        Task<List<ActivityLogDto>> GetActivityLogsAsync(
            DateTime? from,
            DateTime? to,
            [CanBeNull] string[] eventTypes
        );

        Task<List<ActivityLogDto>> GetActivityLogsPagedAsync(
            DateTime? from,
            DateTime? to,
            [CanBeNull] string[] eventTypes,
            int page,
            int pageSize
        );

        Task InsertReportMetadataAsync(ReportMetadataDto metadata);

        Task<List<ReportMetadataDto>> GetReportsMetadataAsync();
    }

}