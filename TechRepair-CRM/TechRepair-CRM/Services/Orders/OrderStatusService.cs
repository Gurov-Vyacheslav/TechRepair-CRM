using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Orders;

public class OrderStatusService : IOrderStatusService
{
    private readonly RepairServiceDbContext _db;
    
    public OrderStatusService(RepairServiceDbContext db)
    {
        _db = db;
    }
    
    public async Task<string?> GetStatusNameAsync(int orderId)
    {
        return await (
            from ro in _db.RepairOrders
            join status in _db.OrderStatuses on ro.StatusId equals status.StatusId
            where ro.OrderId == orderId
            select status.StatusName
        ).SingleOrDefaultAsync();
    }
    
    public async Task<string> EnsureOrderHasStatusToBeModifiedAsync(int orderId)
    {
        var status = await GetStatusNameAsync(orderId);

        if (status is null)
            throw new InvalidOperationException("Заказ не найден.");

        if (status is "Closed" or "Canceled")
            throw new InvalidOperationException("Нельзя изменять закрытый или отменённый заказ.");
        
        return status;
    }
    
    public async Task EnsureOrderIsInRepairAsync(int orderId)
    {
        var status = await GetStatusNameAsync(orderId);

        if (status is null)
            throw new InvalidOperationException("Заказ не найден.");

        if (status != "InRepair")
            throw new InvalidOperationException("Услугу можно завершить только когда заказ находится в статусе InRepair.");
    }
    
    public bool IsTransitionAllowed(string currentStatus, string newStatus)
    {
        return currentStatus switch
        {
            "Created" => newStatus is "Accepted" or "Canceled",
            "Accepted" => newStatus is "InRepair" or "Canceled",
            "InRepair" => newStatus is "Ready" or "Canceled",
            "Ready" => newStatus is "Closed",
            _ => false
        };
    }

    public async Task<OrderStatus> EnsureStatusExistsAsync(string status)
    {
        var newStatusEntity = await _db.OrderStatuses
            .SingleOrDefaultAsync(s => s.StatusName == status);

        return newStatusEntity ?? throw new InvalidOperationException("Новый статус не найден.");
    }
    public async Task<short> GetCreatedStatusIdAsync()
    {
        return await _db.OrderStatuses
            .Where(os => os.StatusName == "Created")
            .Select(os => os.StatusId)
            .SingleAsync();
    }
    
    public async Task EnsureStatusTransitionAsync(int orderId, string newStatus)
    {
        if (newStatus == "InRepair")
        {
            var hasServices = await _db.OrderServices
                .AnyAsync(os => os.OrderId == orderId);

            if (!hasServices)
                throw new InvalidOperationException("Нельзя начать ремонт: в заказе нет услуг.");
        }

        if (newStatus is "Ready" or "Closed")
        {
            var hasServices = await _db.OrderServices
                .AnyAsync(os => os.OrderId == orderId);

            if (!hasServices)
                throw new InvalidOperationException("В заказе нет услуг.");

            var hasUncompletedServices = await _db.OrderServices
                .AnyAsync(os => os.OrderId == orderId && os.CompletedAt == null);

            if (hasUncompletedServices)
                throw new InvalidOperationException("Не все услуги завершены.");
        }

        if (newStatus == "Closed")
        {
            var totalCost = await _db.RepairOrders
                .Where(o => o.OrderId == orderId)
                .Select(o => o.TotalCost)
                .SingleAsync();

            var paidAmount = await _db.Payments
                .Where(p => p.OrderId == orderId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            if (paidAmount < totalCost)
                throw new InvalidOperationException("Нельзя закрыть заказ: он не полностью оплачен.");
        }
    }
}