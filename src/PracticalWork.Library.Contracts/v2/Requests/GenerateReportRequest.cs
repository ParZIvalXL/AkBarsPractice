#nullable enable
namespace PracticalWork.Library.Contracts.v2.Requests;

public record GenerateReportRequest(
    DateTime? From,
    DateTime? To,
    string[]? EventTypes
);