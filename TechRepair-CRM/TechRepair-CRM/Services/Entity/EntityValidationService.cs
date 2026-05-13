using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Auth;
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
    
    public async Task<Service> GetActiveServiceOrThrowAsync(int serviceId)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.IsActive);

        if (service is null)
            throw new InvalidOperationException("Активная услуга не найдена.");

        return service;
    }
    
    public async Task<Part> GetActivePartOrThrowAsync(int partId)
    {
        var part = await _db.Parts
            .FirstOrDefaultAsync(p => p.PartId == partId && p.IsActive);

        if (part is null)
            throw new InvalidOperationException("Активная деталь не найдена.");

        if (part.DefaultPrice is null)
            throw new InvalidOperationException("У детали не указана текущая цена.");

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
    
    public async Task<OrderPart> GetOrderPartOrThrowAsync(int orderId, int partId)
    {
        var orderPart = await _db.OrderParts
            .FirstOrDefaultAsync(op => op.OrderId == orderId && op.PartId == partId);

        if (orderPart is null)
            throw new InvalidOperationException("Деталь в заказе не найдена.");

        return orderPart;
    }
    
    public async Task<Technician> GetTechnicianOrThrowAsync(int id, bool isActive = false)
    {
        var technician = await _db.Technicians.FindAsync(id);

        return technician ?? throw new InvalidOperationException("Мастер не найден.");
    }
    
    public async Task EnsureActiveTechnicianExistsAsync(int technicianId)
    {
        var technicianExists = await _db.Technicians
            .AnyAsync(t => t.TechnicianId == technicianId && t.IsActive);

        if (!technicianExists)
            throw new InvalidOperationException("Активный мастер не найден.");
    }

}