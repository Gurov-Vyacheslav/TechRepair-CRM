using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Technicians;
using TechRepair_CRM.Services.CurrentUser;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Services.Technicians;

public class TechnicianWorkService : ITechnicianWorkService
{
    private readonly RepairServiceDbContext _db;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrderStatusService _orderStatusService;

    public TechnicianWorkService(
        RepairServiceDbContext db,
        ICurrentUserService currentUserService,
        IOrderStatusService orderStatusService)
    {
        _db = db;
        _currentUserService = currentUserService;
        _orderStatusService = orderStatusService;
    }

    public async Task<IReadOnlyList<MyWorkItemResponse>> GetMyWorkAsync(bool onlyActive = true)
    {
        var technicianId = await _currentUserService.GetTechnicianIdAsync();
        if (technicianId is null)
            throw new InvalidOperationException("Текущий пользователь не привязан к мастеру.");

        return await GetWorkItemsAsync(technicianId.Value, onlyActive);
    }
    
    private async Task<IReadOnlyList<MyWorkItemResponse>> GetWorkItemsAsync(
        int technicianId, 
        bool onlyActive)
    {
        var query =
            from os in _db.OrderServices
            join ro in _db.RepairOrders on os.OrderId equals ro.OrderId
            join status in _db.OrderStatuses on ro.StatusId equals status.StatusId
            join service in _db.Services on os.ServiceId equals service.ServiceId
            join device in _db.Devices on ro.DeviceId equals device.DeviceId
            join deviceType in _db.DeviceTypes on device.DeviceTypeId equals deviceType.DeviceTypeId
            join client in _db.Clients on device.ClientId equals client.ClientId
            where os.TechnicianId == technicianId
            select new
            {
                os,
                ro,
                status,
                service,
                device,
                deviceType,
                client
            };

        if (onlyActive)
        {
            query = query.Where(x =>
                x.os.CompletedAt == null &&
                x.status.StatusName != "Closed" &&
                x.status.StatusName != "Canceled");
        }

        return await query
            .OrderByDescending(x => x.ro.CreatedAt)
            .ThenBy(x => x.ro.OrderNumber)
            .Select(x => new MyWorkItemResponse(
                x.ro.OrderId,
                x.ro.OrderNumber,
                x.ro.CreatedAt,
                x.status.StatusName,
                x.client.LastName + " " + x.client.FirstName,
                x.client.Phone,
                x.deviceType.TypeName,
                x.device.Brand,
                x.device.Model,
                x.os.ServiceId,
                x.service.ServiceName,
                x.os.Quantity,
                x.os.PriceAtMoment,
                x.os.Quantity * x.os.PriceAtMoment,
                x.os.CompletedAt,
                x.os.Notes
            ))
            .ToListAsync();
    }


    public async Task CompleteMyServiceAsync(int orderId, int serviceId)
    {
        var technicianId = await _currentUserService.GetTechnicianIdAsync();

        if (technicianId is null)
            throw new InvalidOperationException("Текущий пользователь не привязан к мастеру.");

        await _orderStatusService.EnsureOrderIsInRepairAsync(orderId);

        var orderService = await _db.OrderServices
            .FirstOrDefaultAsync(os =>
                os.OrderId == orderId &&
                os.ServiceId == serviceId &&
                os.TechnicianId == technicianId.Value);

        if (orderService is null)
            throw new InvalidOperationException("Услуга не найдена или не назначена текущему мастеру.");

        if (orderService.CompletedAt is not null)
            return;

        orderService.CompletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }
    
    public async Task<IReadOnlyList<OrderListItemResponse>> GetMyOrdersAsync(bool onlyActive = true)
    {
        var technicianId = await _currentUserService.GetTechnicianIdAsync();

        if (technicianId is null)
            throw new InvalidOperationException("Текущий пользователь не привязан к мастеру.");

        return await GetTechnicianOrdersAsync(technicianId.Value, onlyActive);
    }
    
    public async Task<IReadOnlyList<OrderListItemResponse>> GetTechnicianOrdersAsync(int technicianId, bool onlyActive = false)
    {
        var query = _db.VwOrderFullInfos
            .Where(o => _db.OrderServices.Any(os =>
                os.OrderId == o.OrderId &&
                os.TechnicianId == technicianId));

        if (onlyActive)
        {
            query = query.Where(o =>
                o.OrderStatus != "Closed" &&
                o.OrderStatus != "Canceled");
        }

        return await OrderListProjection.GetOrdersListItems(query);
    }
    
    public async Task<TechnicianProfileResponse?> GetTechnicianProfileAsync(int technicianId)
    {
        return await _db.Technicians
            .Where(t => t.TechnicianId == technicianId)
            .Select(t => new TechnicianProfileResponse(
                t.TechnicianId,
                t.FirstName,
                t.LastName,
                t.Email,
                t.Phone,
                t.Specialization,
                t.IsActive,
                t.Notes
            ))
            .FirstOrDefaultAsync();
    }
    
    public async Task<IReadOnlyList<MyWorkItemResponse>> GetTechnicianWorkAsync(
        int technicianId, 
        bool onlyActive = false)
    {
        return await GetWorkItemsAsync(technicianId, onlyActive);
    }
}
