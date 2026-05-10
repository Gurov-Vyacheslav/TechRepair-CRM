using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderCommandService
{
    Task<int> CreateOrderAsync(CreateOrderRequest request);
    Task UpdateOrderAsync(int orderId, OrderEditRequest request);
    Task AddServiceToOrderAsync(int orderId, AddOrderServiceRequest request);
    Task AddPartToOrderAsync(int orderId, AddOrderPartRequest request);
    Task CompleteServiceAsync(int orderId, int serviceId);
    Task AddPaymentAsync(int orderId, AddPaymentRequest request);
    Task ChangeStatusAsync(int orderId, string newStatus, string? comment);
}