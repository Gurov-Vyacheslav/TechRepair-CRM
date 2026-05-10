using TechRepair_CRM.DTOs.Orders;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderQueryService
{
    Task<List<OrderListItemResponse>> GetOrdersAsync(OrderFilterRequest? filter = null);
    Task<OrderDetailsResponse?> GetOrderDetailsAsync(int orderId);
    Task<OrderEditRequest?> GetOrderEditFormAsync(int orderId);
}