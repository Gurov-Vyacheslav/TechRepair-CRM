using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Technicians;

namespace TechRepair_CRM.Services.Technicians;

public interface ITechnicianWorkService
{
    Task<IReadOnlyList<MyWorkItemResponse>> GetMyWorkAsync(bool onlyActive = true);

    Task CompleteMyServiceAsync(int orderId, int serviceId);
    
    Task<IReadOnlyList<OrderListItemResponse>> GetMyOrdersAsync(bool onlyActive = true);

    Task<IReadOnlyList<OrderListItemResponse>> GetTechnicianOrdersAsync(int technicianId, bool onlyActive = false);
    
    Task<TechnicianProfileResponse?> GetTechnicianProfileAsync(int technicianId);

    Task<IReadOnlyList<MyWorkItemResponse>> GetTechnicianWorkAsync(int technicianId, bool onlyActive = false);
}