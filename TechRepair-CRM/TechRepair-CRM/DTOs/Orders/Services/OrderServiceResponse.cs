using TechRepair_CRM.DTOs.Orders.Services.Parts;

namespace TechRepair_CRM.DTOs.Orders.Services;

public record OrderServiceResponse(
    int OrderId,
    int ServiceId,
    string ServiceName,
    int? TechnicianId,
    string? TechnicianFullName,
    short Quantity,
    decimal PriceAtMoment,
    decimal TotalPrice,
    DateTime? CompletedAt,
    string? Notes,
    IReadOnlyList<OrderServicePartResponse> Parts
);