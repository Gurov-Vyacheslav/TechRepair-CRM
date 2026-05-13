using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.Models.Db;
using TechRepair_CRM.Services.CurrentUser;
using TechRepair_CRM.Services.Entity;

namespace TechRepair_CRM.Services.Orders;

public class OrderCommandService : IOrderCommandService
{
    private readonly RepairServiceDbContext _db;
    private readonly IOrderStatusService  _orderStatusService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEntityValidationService _entityValidationService;

    public OrderCommandService(
        RepairServiceDbContext db,
        IOrderStatusService  orderStatusService,
        ICurrentUserService currentUserService,
        IEntityValidationService entityValidationService)
    {
        _db = db;
        _orderStatusService = orderStatusService;
        _currentUserService = currentUserService;
        _entityValidationService = entityValidationService;
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request)
    {
        await _entityValidationService.EnsureDeviceExistsAsync(request.DeviceId);

        var createdStatusId = await _orderStatusService.GetCreatedStatusIdAsync();

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
    
    public async Task UpdateOrderAsync(int orderId, OrderEditRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);

        var order = await _entityValidationService.GetOrderOrThrowAsync(orderId);

        order.ProblemDescription = request.ProblemDescription;
        order.DiagnosticResult = request.DiagnosticResult;
        order.EstimatedCost = request.EstimatedCost;
        order.WarrantyMonths = request.WarrantyMonths;
        order.IsWarrantyRepair = request.IsWarrantyRepair;
        order.Notes = request.Notes;

        await _db.SaveChangesAsync();
    }
    
    private void EnsureCurrentUserIsAdminOrManager()
    {
        if (!_currentUserService.IsAdminOrManager())
            throw new InvalidOperationException("Это действие доступно только администратору или менеджеру.");
    }

    public async Task AddServiceToOrderAsync(int orderId, AddOrderServiceRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);
        await _entityValidationService.EnsureOrderServiceNotExistsAsync(orderId, request.ServiceId);

        var service = await _entityValidationService.GetActiveServiceOrThrowAsync(request.ServiceId);

        if (request.TechnicianId.HasValue)
        {
            await _entityValidationService.EnsureActiveTechnicianExistsAsync(request.TechnicianId.Value);
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
    
    public async Task AddPartToOrderAsync(int orderId, AddOrderPartRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);

        var existingOrderPart = await GetOrderPartAsync(orderId, request.PartId);

        if (existingOrderPart is not null)
        {
            existingOrderPart.Quantity += request.Quantity;
        }
        else
        {
            var part = await _entityValidationService.GetActivePartOrThrowAsync(request.PartId);
            
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

    private async Task<OrderPart?> GetOrderPartAsync(int orderId, int partId)
    {
        return await _db.OrderParts
            .FirstOrDefaultAsync(op =>
                op.OrderId == orderId &&
                op.PartId == partId);
    }
    

    public async Task CompleteServiceAsync(int orderId, int serviceId)
    {
        await EnsureCurrentUserCanCompleteServiceAsync(orderId, serviceId);
        await _orderStatusService.EnsureOrderIsInRepairAsync(orderId);

        var orderService = await _entityValidationService.GetOrderServiceOrThrowAsync(orderId, serviceId);

        if (orderService.CompletedAt is not null)
            return;

        orderService.CompletedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }
    
    private async Task EnsureCurrentUserCanCompleteServiceAsync(int orderId, int serviceId)
    {
        if (_currentUserService.IsAdminOrManager())
            return;

        if (!_currentUserService.IsTechnician())
            throw new InvalidOperationException("Недостаточно прав для завершения услуги.");

        var technicianId = await _currentUserService.GetTechnicianIdAsync();

        if (technicianId is null)
            throw new InvalidOperationException("Текущий пользователь не привязан к мастеру.");

        var isAssigned = await _db.OrderServices.AnyAsync(os =>
            os.OrderId == orderId &&
            os.ServiceId == serviceId &&
            os.TechnicianId == technicianId.Value);

        if (!isAssigned)
            throw new InvalidOperationException("Нельзя завершить услугу, которая не назначена текущему мастеру.");
    }
    

    public async Task AddPaymentAsync(int orderId, AddPaymentRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _entityValidationService.GetOrderOrThrowAsync(orderId);

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
        EnsureCurrentUserIsAdminOrManager();

        var order = await _entityValidationService.GetOrderOrThrowAsync(orderId);

        var currentStatus = await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);

        if (currentStatus == newStatus)
            return;

        if (!_orderStatusService.IsTransitionAllowed(currentStatus, newStatus))
            throw new InvalidOperationException($"Недопустимый переход статуса: {currentStatus} → {newStatus}.");

        await _orderStatusService.EnsureStatusTransitionAsync(orderId, newStatus);

        var newStatusEntity = await _orderStatusService.EnsureStatusExistsAsync(newStatus);

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

        await _db.SaveChangesAsync();
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
    
    public async Task UpdateOrderServiceAsync(int orderId, int serviceId, EditOrderServiceRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);

        var orderService = await _entityValidationService.GetOrderServiceOrThrowAsync(orderId, serviceId, true);

        if (request.TechnicianId.HasValue)
        {
            await _entityValidationService.EnsureActiveTechnicianExistsAsync(request.TechnicianId.Value);
        }

        orderService.Quantity = request.Quantity;
        orderService.TechnicianId = request.TechnicianId;
        orderService.Notes = request.Notes;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateOrderPartAsync(int orderId, int partId, EditOrderPartRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);

        var orderPart = await _entityValidationService.GetOrderPartOrThrowAsync(orderId, partId);

        orderPart.Quantity = request.Quantity;

        await _db.SaveChangesAsync();
    }

}