namespace TechRepair_CRM.DTOs.Orders;

public record CreateOrderResponse(
    int OrderId,
    string OrderNumber
);