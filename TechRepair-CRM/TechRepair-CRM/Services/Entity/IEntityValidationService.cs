using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Entity;

public interface IEntityValidationService
{
    Task EnsureDeviceTypeExistsAsync(int deviceTypeId);
    Task<Client> GetClientOrThrowAsync(int clientId);
    Task<Device> GetDeviceOrThrowAsync(int deviceId);
    Task EnsureDeviceExistsAsync(int deviceId);
    Task<RepairOrder> GetOrderOrThrowAsync(int orderId);
    Task<Service> GetActiveServiceOrThrowAsync(int serviceId);
    Task<Part> GetActivePartOrThrowAsync(int partId);
    Task<OrderService> GetOrderServiceOrThrowAsync(int orderId, int serviceId, bool notCompleted = false);
    Task EnsureOrderServiceNotExistsAsync(int orderId, int serviceId);
    Task<OrderPart> GetOrderPartOrThrowAsync(int orderId, int partId);
    Task<Technician> GetTechnicianOrThrowAsync(int id, bool isActive = false);
    Task EnsureActiveTechnicianExistsAsync(int technicianId);
}