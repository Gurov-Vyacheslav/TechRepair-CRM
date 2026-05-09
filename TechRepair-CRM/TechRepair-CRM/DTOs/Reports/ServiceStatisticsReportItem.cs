namespace TechRepair_CRM.DTOs.Reports;

public record ServiceStatisticsReportItem(
    int ServiceId,
    string ServiceName,
    bool IsActive,
    long UsageCount,
    long TotalQuantity,
    decimal TotalRevenue,
    decimal? AvgPriceAtMoment
);