using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.DTOs.Orders.Services;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderQueryService
{
    Task<List<OrderListItemResponse>> GetOrdersAsync(OrderFilterRequest? filter = null);
    Task<OrderDetailsResponse?> GetOrderDetailsAsync(int orderId);
    Task<OrderEditRequest?> GetOrderEditFormAsync(int orderId);
    
    Task<string?> GetOrderStatusAsync(int orderId);
    
    Task<EditOrderServiceRequest?> GetOrderServiceEditFormAsync(int orderId, int serviceId);

    Task<EditOrderPartRequest?> GetOrderPartEditFormAsync(int orderId, int partId);
}