namespace TechRepair_CRM.DTOs.Technicians;

public record MyWorkItemResponse(
    int OrderId,
    string OrderNumber,
    DateTime CreatedAt,
    string OrderStatus,
    string ClientFullName,
    string ClientPhone,
    string DeviceType,
    string? Brand,
    string? Model,
    int ServiceId,
    string ServiceName,
    short Quantity,
    decimal PriceAtMoment,
    decimal TotalPrice,
    DateTime? CompletedAt,
    string? Notes
);