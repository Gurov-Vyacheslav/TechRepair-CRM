namespace TechRepair_CRM.DTOs.Orders;

public record OrderListItemResponse(
    int OrderId,
    string OrderNumber,
    DateTime CreatedAt,
    string OrderStatus,
    string ClientFullName,
    string ClientPhone,
    string DeviceType,
    string? Brand,
    string? Model,
    bool IsWarrantyRepair,
    decimal TotalCost,
    decimal PaidAmount,
    decimal RemainingAmount
);