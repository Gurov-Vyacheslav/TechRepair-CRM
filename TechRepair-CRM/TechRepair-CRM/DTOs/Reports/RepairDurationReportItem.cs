namespace TechRepair_CRM.DTOs.Reports;

public record RepairDurationReportItem(
    int OrderId,
    string OrderNumber,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string OrderStatus,
    decimal? RepairDurationHours,
    decimal? RepairDurationDays
);