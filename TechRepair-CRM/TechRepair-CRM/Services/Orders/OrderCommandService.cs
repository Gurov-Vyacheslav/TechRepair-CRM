using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Orders;

public class OrderCommandService : IOrderCommandService
{
    private readonly RepairServiceDbContext _db;

    public OrderCommandService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request)
    {
        await CheckDeviceExistsAsync(request.DeviceId);

        var createdStatusId = await GetCreatedStatusIdAsync();

        var orderId = await GetNextOrderIdAsync();

        var order = new RepairOrder
        {
            OrderId = orderId,
            OrderNumber = $"RO-{DateTime.Today:yyyyMMdd}-{orderId:D6}",
            DeviceId = request.DeviceId,
            StatusId = createdStatusId,
            CreatedAt = DateTime.Now,
            ProblemDescription = request.ProblemDescription,
            DiagnosticResult = request.DiagnosticResult,
            EstimatedCost = request.EstimatedCost,
            TotalCost = 0,
            WarrantyMonths = request.WarrantyMonths,
            IsWarrantyRepair = request.IsWarrantyRepair,
            Notes = request.Notes
        };

        _db.RepairOrders.Add(order);
        await _db.SaveChangesAsync();

        return order.OrderId;
    }

    private async Task CheckDeviceExistsAsync(int deviceId)
    {
        var deviceExists = await _db.Devices
            .AnyAsync(d => d.DeviceId == deviceId);

        if (!deviceExists)
            throw new InvalidOperationException("Устройство не найдено.");
    }

    private async Task<short> GetCreatedStatusIdAsync()
    {
        return await _db.OrderStatuses
            .Where(os => os.StatusName == "Created")
            .Select(os => os.StatusId)
            .SingleAsync();
    }
    
    public async Task UpdateOrderAsync(int orderId, OrderEditRequest request)
    {
        await EnsureOrderCanBeModifiedAsync(orderId);

        var order = await CheckOrderExistsAsync(orderId);

        order.ProblemDescription = request.ProblemDescription;
        order.DiagnosticResult = request.DiagnosticResult;
        order.EstimatedCost = request.EstimatedCost;
        order.WarrantyMonths = request.WarrantyMonths;
        order.IsWarrantyRepair = request.IsWarrantyRepair;
        order.Notes = request.Notes;

        await _db.SaveChangesAsync();
    }
    
    private async Task<RepairOrder> CheckOrderExistsAsync(int orderId)
    {
        var order = await _db.RepairOrders.FindAsync(orderId);

        return order ?? throw new InvalidOperationException("Заказ не найден.");
    }

    public async Task AddServiceToOrderAsync(int orderId, AddOrderServiceRequest request)
    {
        await EnsureOrderCanBeModifiedAsync(orderId);

        var service = await CheckActiveServiceExistsAsync(request.ServiceId);

        if (request.TechnicianId.HasValue)
        {
            await CheckActiveTechnicianExistsAsync(request.TechnicianId.Value);
        }

        var price = service.BasePrice;

        var orderService = new OrderService
        {
            OrderId = orderId,
            ServiceId = request.ServiceId,
            TechnicianId = request.TechnicianId,
            Quantity = request.Quantity,
            PriceAtMoment = price,
            Notes = request.Notes
        };

        _db.OrderServices.Add(orderService);
        await _db.SaveChangesAsync();
    }

    private async Task<Service> CheckActiveServiceExistsAsync(int serviceId)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.IsActive);

        if (service is null)
            throw new InvalidOperationException("Активная услуга не найдена.");

        return service;
    }

    private async Task CheckActiveTechnicianExistsAsync(int technicianId)
    {
        var technicianExists = await _db.Technicians
            .AnyAsync(t => t.TechnicianId == technicianId && t.IsActive);

        if (!technicianExists)
            throw new InvalidOperationException("Активный мастер не найден.");
    }
    
    public async Task AddPartToOrderAsync(int orderId, AddOrderPartRequest request)
    {
        await EnsureOrderCanBeModifiedAsync(orderId);

        var existingOrderPart = await GetOrderPartAsync(orderId, request.PartId);

        if (existingOrderPart is not null)
        {
            existingOrderPart.Quantity += request.Quantity;
        }
        else
        {
            var part = await CheckActivePartExistsAsync(request.PartId);
            
            var orderPart = new OrderPart
            {
                OrderId = orderId,
                PartId = request.PartId,
                Quantity = request.Quantity,
                PriceAtMoment = part.DefaultPrice.Value
            };

            _db.OrderParts.Add(orderPart);
        }

        await _db.SaveChangesAsync();
    }

    private async Task<Part> CheckActivePartExistsAsync(int partId)
    {
        var part = await _db.Parts
            .FirstOrDefaultAsync(p => p.PartId == partId && p.IsActive);

        if (part is null)
            throw new InvalidOperationException("Активная деталь не найдена.");

        if (part.DefaultPrice is null)
            throw new InvalidOperationException("У детали не указана текущая цена.");

        return part;
    }

    private async Task<OrderPart?> GetOrderPartAsync(int orderId, int partId)
    {
        return await _db.OrderParts
            .FirstOrDefaultAsync(op =>
                op.OrderId == orderId &&
                op.PartId == partId);
    }

    public async Task CompleteServiceAsync(int orderId, int serviceId)
    {
        var status = await GetOrderStatusAsync(orderId);

        if (status is null)
            throw new InvalidOperationException("Заказ не найден.");

        if (status != "InRepair")
            throw new InvalidOperationException("Услугу можно завершить только когда заказ находится в статусе InRepair.");

        var orderService = await GetOrderServiceAsync(orderId, serviceId);

        if (orderService is null)
            throw new InvalidOperationException("Услуга в заказе не найдена.");

        if (orderService.CompletedAt is not null)
            return;

        orderService.CompletedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    private async Task<string?> GetOrderStatusAsync(int orderId)
    {
        return await _db.RepairOrders
            .Where(o => o.OrderId == orderId)
            .Select(o => o.Status.StatusName)
            .SingleOrDefaultAsync();
    }

    private async Task<OrderService?> GetOrderServiceAsync(int orderId, int serviceId)
    {
        return await _db.OrderServices
            .FirstOrDefaultAsync(os => os.OrderId == orderId && os.ServiceId == serviceId);
    }

    public async Task AddPaymentAsync(int orderId, AddPaymentRequest request)
    {
        await CheckOrderExistsAsync(orderId);

        var payment = new Payment
        {
            OrderId = orderId,
            PaymentDate = DateTime.Now,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            TransactionNumber = request.TransactionNumber,
            Notes = request.Notes
        };

        _db.Payments.Add(payment);

        // Переплату дополнительно защитит триггер БД.
        await _db.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int orderId, string newStatus, string? comment)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var order = await CheckOrderExistsAsync(orderId);

        var currentStatus = await GetOrderStatusAsync(orderId);
        if (currentStatus is null)
            throw new InvalidOperationException("Статус заказа не найден");

        if (currentStatus == newStatus)
            return;

        if (currentStatus is "Closed" or "Canceled")
            throw new InvalidOperationException("Нельзя менять статус закрытого или отменённого заказа.");

        if (!IsTransitionAllowed(currentStatus, newStatus))
            throw new InvalidOperationException($"Недопустимый переход статуса: {currentStatus} → {newStatus}.");

        await ValidateStatusTransitionAsync(orderId, newStatus);

        var newStatusEntity = await CheckStatusExistsAsync(newStatus);

        order.StatusId = newStatusEntity.StatusId;
        
        var now = DateTime.Now;
        UpdateTimestamps(order, newStatus, now);
        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            StatusId = newStatusEntity.StatusId,
            ChangedAt = now,
            Comment = comment
        });

        // Если в БД остался триггер, который разрешает менять status_id
        // только при наличии app.allow_status_change, эта строка нужна.
        // Если такого триггера нет, она не мешает.
        await _db.Database.ExecuteSqlRawAsync(
            "SELECT set_config('app.allow_status_change', 'on', true)");

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<OrderStatus> CheckStatusExistsAsync(string status)
    {
        var newStatusEntity = await _db.OrderStatuses
            .SingleOrDefaultAsync(s => s.StatusName == status);

        return newStatusEntity ?? throw new InvalidOperationException("Новый статус не найден.");
    }

    private static void UpdateTimestamps(RepairOrder order, string newStatus, DateTime now)
    {
        switch (newStatus)
        {
            case "Accepted" when order.AcceptedAt is null:
                order.AcceptedAt = now;
                break;
            case "Ready" when order.CompletedAt is null:
                order.CompletedAt = now;
                break;
            case "Closed" when order.IssuedAt is null:
                order.IssuedAt = now;
                break;
        }
    }

    private async Task EnsureOrderCanBeModifiedAsync(int orderId)
    {
        var status = await _db.RepairOrders
            .Where(o => o.OrderId == orderId)
            .Join(
                _db.OrderStatuses,
                order => order.StatusId,
                status => status.StatusId,
                (order, status) => status.StatusName)
            .SingleOrDefaultAsync();

        if (status is null)
            throw new InvalidOperationException("Заказ не найден.");

        if (status is "Closed" or "Canceled")
            throw new InvalidOperationException("Нельзя изменять закрытый или отменённый заказ.");
    }

    private static bool IsTransitionAllowed(string currentStatus, string newStatus)
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

    private async Task ValidateStatusTransitionAsync(int orderId, string newStatus)
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

    private async Task<int> GetNextOrderIdAsync()
    {
        var connection = _db.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT nextval(pg_get_serial_sequence('repair_order', 'order_id'))";

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}