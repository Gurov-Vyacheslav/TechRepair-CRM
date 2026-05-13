using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Orders;

public interface IOrderStatusService
{
    Task<string?> GetStatusNameAsync(int orderId);
    Task<string> EnsureOrderHasStatusToBeModifiedAsync(int orderId);
    Task EnsureOrderIsInRepairAsync(int orderId);
    bool IsTransitionAllowed(string currentStatus, string newStatus);
    Task<OrderStatus> EnsureStatusExistsAsync(string status);
    Task<short> GetCreatedStatusIdAsync();
    Task EnsureStatusTransitionAsync(int orderId, string newStatus);
}