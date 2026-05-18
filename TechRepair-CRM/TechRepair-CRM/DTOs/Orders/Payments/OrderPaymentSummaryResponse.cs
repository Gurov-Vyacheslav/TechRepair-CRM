namespace TechRepair_CRM.DTOs.Orders.Payments;

public record OrderPaymentSummaryResponse(
    int OrderId,
    string OrderNumber,
    bool IsWarrantyRepair,
    decimal TotalCost,
    decimal PaidAmount,
    decimal RemainingAmount,
    decimal RequiredToCloseAmount
);