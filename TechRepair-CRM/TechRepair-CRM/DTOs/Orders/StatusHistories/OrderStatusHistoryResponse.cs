namespace TechRepair_CRM.DTOs.Orders.StatusHistory;

public record OrderStatusHistoryResponse(
    int HistoryId,
    int OrderId,
    string StatusName,
    DateTime ChangedAt,
    string? Comment
);