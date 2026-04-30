namespace TechRepair_CRM.DTOs.Orders;

public record OrderDetailsResponse(
    int OrderId,
    string OrderNumber,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? CompletedAt,
    DateTime? IssuedAt,

    string OrderStatus,

    int ClientId,
    string ClientFirstName,
    string ClientLastName,
    string ClientPhone,
    string? ClientEmail,

    int DeviceId,
    string DeviceType,
    string? Brand,
    string? Model,
    string? SerialNumber,
    string? EquipmentDescription,
    string? ExternalCondition,

    string ProblemDescription,
    string? DiagnosticResult,
    decimal EstimatedCost,
    decimal TotalCost,
    bool IsWarrantyRepair,
    short? WarrantyMonths,
    string? Notes,

    decimal ServiceSum,
    decimal PartSum,
    decimal PaidAmount,
    decimal RemainingAmount
);