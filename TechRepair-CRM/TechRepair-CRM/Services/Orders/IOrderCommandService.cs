using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.DTOs.Orders.Services.Parts;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderCommandService
{
    Task<int> CreateOrderAsync(CreateOrderRequest request);
    Task UpdateOrderAsync(int orderId, OrderEditRequest request);
    Task AddServiceToOrderAsync(int orderId, AddOrderServiceRequest request);
    Task AddPartToOrderServiceAsync(int orderId, int serviceId, AddOrderServicePartRequest request);
    Task CompleteServiceAsync(int orderId, int serviceId);
    Task AddPaymentAsync(int orderId, AddPaymentRequest request);
    Task ChangeStatusAsync(int orderId, string newStatus, string? comment);
    Task UpdateOrderServiceAsync(int orderId, int serviceId, EditOrderServiceRequest request);
    Task UpdateOrderServicePartAsync(
        int orderId,
        int serviceId,
        int partId,
        EditOrderServicePartRequest request);
}