using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.DTOs.Orders.Services.Parts;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderQueryService
{
    Task<IReadOnlyList<OrderListItemResponse>> GetOrdersAsync(OrderFilterRequest? filter = null);
    Task<OrderDetailsResponse?> GetOrderDetailsAsync(int orderId);
    Task<OrderEditRequest?> GetOrderEditFormAsync(int orderId);
    
    Task<EditOrderServiceRequest?> GetOrderServiceEditFormAsync(int orderId, int serviceId);

    Task<EditOrderServicePartRequest?> GetOrderServicePartEditFormAsync(int orderId, int serviceId, int partId);
    
    Task<OrderPaymentSummaryResponse?> GetPaymentSummaryAsync(int orderId);
}