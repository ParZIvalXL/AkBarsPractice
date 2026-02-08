namespace PracticalWork.Reports.Data.PostgreSql.Entities;

public class ReportMetadataEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}