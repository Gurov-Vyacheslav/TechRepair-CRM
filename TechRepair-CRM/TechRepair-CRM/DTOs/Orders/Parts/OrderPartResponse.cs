namespace TechRepair_CRM.DTOs.Orders.Parts;

public record OrderPartResponse(
    int OrderId,
    int PartId,
    string PartName,
    string? PartNumber,
    int Quantity,
    decimal PriceAtMoment,
    decimal TotalPrice
);