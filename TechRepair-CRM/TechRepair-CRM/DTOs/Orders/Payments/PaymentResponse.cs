namespace TechRepair_CRM.DTOs.Orders.Payments;

public record PaymentResponse(
    int PaymentId,
    int OrderId,
    DateTime PaymentDate,
    decimal Amount,
    string PaymentMethod,
    string? TransactionNumber,
    string? Notes
);