namespace TechRepair_CRM.DTOs.Orders.Services.Parts;

public record OrderServicePartResponse(
    int OrderId,
    int ServiceId,
    int PartId,
    string PartName,
    string? PartNumber,
    int Quantity,
    decimal PriceAtMoment,
    decimal TotalPrice
);