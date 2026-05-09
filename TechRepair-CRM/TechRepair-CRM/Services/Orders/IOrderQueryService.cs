using TechRepair_CRM.DTOs;
using TechRepair_CRM.DTOs.Orders;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderQueryService
{
    Task<List<OrderListItemResponse>> GetOrdersAsync();

    Task<OrderDetailsResponse?> GetOrderDetailsAsync(int orderId);
}