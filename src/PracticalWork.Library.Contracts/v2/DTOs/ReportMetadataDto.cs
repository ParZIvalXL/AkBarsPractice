namespace PracticalWork.Library.Contracts.v2.DTOs;

public class ReportMetadataDto
{
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public long SizeBytes { get; init; }
}