namespace TechRepair_CRM.DTOs.Orders;

public sealed record CreateOrderRequest(
    int DeviceId,
    string ProblemDescription,
    decimal EstimatedCost,
    short? WarrantyMonths,
    bool IsWarrantyRepair,
    string? DiagnosticResult,
    string? Notes);