namespace TechRepair_CRM.DTOs.Reports;

public record PartUsageStatisticsReportItem(
    int PartId,
    string? PartNumber,
    string PartName,
    string? Manufacturer,
    bool IsActive,
    long UsageCount,
    long TotalQuantity,
    decimal TotalAmount
);