using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Entity;

public class EntityValidationService:IEntityValidationService
{
    private readonly RepairServiceDbContext _db;
    
    public EntityValidationService(
        RepairServiceDbContext db)
    {
        _db = db;
    }
    
    
    public async Task EnsureDeviceTypeExistsAsync(int deviceTypeId)
    {
        var deviceTypeExists = await _db.DeviceTypes
            .AnyAsync(t => t.DeviceTypeId == deviceTypeId);

        if (!deviceTypeExists)
            throw new InvalidOperationException("Тип устройства не найден.");
    }
    
    public async Task<Client> GetClientOrThrowAsync(int clientId)
    {
        var client = await _db.Clients.FindAsync(clientId);
        return client ?? throw new InvalidOperationException("Клиент не найден.");
    }

    public async Task EnsureClientExistsAsync(int clientId)
    {
        await GetClientOrThrowAsync(clientId);
    }
    
    public async Task<Device> GetDeviceOrThrowAsync(int deviceId)
    {
        var device = await _db.Devices.FindAsync(deviceId);

        return device ?? throw new InvalidOperationException("Устройство не найдено.");
    }
    
    public async Task EnsureDeviceExistsAsync(int deviceId)
    {
        await GetDeviceOrThrowAsync(deviceId);
    }
    
    public async Task<RepairOrder> GetOrderOrThrowAsync(int orderId)
    {
        var order = await _db.RepairOrders.FindAsync(orderId);

        return order ?? throw new InvalidOperationException("Заказ не найден.");
    }

    public async Task EnsureOrderExistsAsync(int orderId)
    {
        await GetOrderOrThrowAsync(orderId);
    }
    
    public async Task<Service> GetServiceOrThrowAsync(int serviceId, bool isActive = false)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

        if (service is null)
            throw new InvalidOperationException("Услуга не найдена.");
        
        if (isActive && !service.IsActive)
            throw new InvalidOperationException("Услуга не доступна.");
        
        return service;
    }
    
    public async Task<Part> GetPartOrThrowAsync(int partId, bool isActive = false)
    {
        var part = await _db.Parts
            .FirstOrDefaultAsync(p => p.PartId == partId);
        
        if (part is null)
            throw new InvalidOperationException("Деталь не найдена.");
        
        if (isActive && !part.IsActive)
            throw new InvalidOperationException("Деталь не доступна.");

        return part;
    }
    
    public async Task<OrderService> GetOrderServiceOrThrowAsync(int orderId, int serviceId, bool notCompleted = false)
    {
        var orderService = await _db.OrderServices
            .FirstOrDefaultAsync(os => os.OrderId == orderId && os.ServiceId == serviceId);

        if (orderService is null)
            throw new InvalidOperationException("Услуга в заказе не найдена.");
        
        if (notCompleted && orderService.CompletedAt is not null)
            throw new InvalidOperationException("Нельзя редактировать уже завершённую услугу.");

        return orderService;
    }
    
    public async Task EnsureOrderServiceNotExistsAsync(int orderId, int serviceId)
    {
        var serviceAlreadyAdded = await _db.OrderServices
            .AnyAsync(os =>
                os.OrderId == orderId &&
                os.ServiceId == serviceId);

        if (serviceAlreadyAdded)
            throw new InvalidOperationException("Эта услуга уже добавлена к заказу.");
    }
    
    public async Task<OrderServicePart> GetOrderServicePartOrThrowAsync(int orderId, int serviceId, int partId)
    {
        var orderPart = await _db.OrderServiceParts
            .FirstOrDefaultAsync(osp =>
                osp.OrderId == orderId &&
                osp.ServiceId == serviceId &&
                osp.PartId == partId);
        
        return orderPart ?? throw new InvalidOperationException("Деталь в заказе не найдена.");
    }
    
    public async Task<Technician> GetTechnicianOrThrowAsync(int id, bool isActive = false)
    {
        var technician = await _db.Technicians.FindAsync(id);

        if (technician is null)
            throw new InvalidOperationException("Мастер не найден.");
        
        if (isActive && !technician.IsActive) 
            throw new InvalidOperationException("Мастер не доступен.");
        
        return technician;
    }
    
    public async Task EnsureActiveTechnicianExistsAsync(int technicianId)
    {
        await GetTechnicianOrThrowAsync(technicianId, true);
    }

}