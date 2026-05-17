using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Entity;

public interface IEntityValidationService
{
    Task EnsureDeviceTypeExistsAsync(int deviceTypeId);
    Task<Client> GetClientOrThrowAsync(int clientId);
    Task EnsureClientExistsAsync(int clientId);
    Task<Device> GetDeviceOrThrowAsync(int deviceId);
    Task EnsureDeviceExistsAsync(int deviceId);
    Task<RepairOrder> GetOrderOrThrowAsync(int orderId);
    Task EnsureOrderExistsAsync(int orderId);
    Task<Service> GetServiceOrThrowAsync(int serviceId, bool isActive = false);
    Task<Part> GetPartOrThrowAsync(int partId, bool isActive = false);
    Task<OrderService> GetOrderServiceOrThrowAsync(int orderId, int serviceId, bool notCompleted = false);
    Task EnsureOrderServiceNotExistsAsync(int orderId, int serviceId);
    Task<OrderServicePart> GetOrderServicePartOrThrowAsync(int orderId, int serviceId, int partId);
    Task<Technician> GetTechnicianOrThrowAsync(int id, bool isActive = false);
    Task EnsureActiveTechnicianExistsAsync(int technicianId);
}