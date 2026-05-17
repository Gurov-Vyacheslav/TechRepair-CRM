using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.DTOs.Orders.Services.Parts;
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

        var service = await _entityValidationService.GetServiceOrThrowAsync(request.ServiceId, true);

        if (request.TechnicianId.HasValue)
        {
            await _entityValidationService.EnsureActiveTechnicianExistsAsync(request.TechnicianId.Value);
        }

        var orderService = new OrderService
        {
            OrderId = orderId,
            ServiceId = request.ServiceId,
            TechnicianId = request.TechnicianId,
            Quantity = request.Quantity,
            PriceAtMoment = service.BasePrice,
            Notes = request.Notes
        };

        _db.OrderServices.Add(orderService);
        await _db.SaveChangesAsync();
    }
    
    public async Task AddPartToOrderServiceAsync(
        int orderId,
        int serviceId,
        AddOrderServicePartRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);
        
        await _entityValidationService.GetOrderServiceOrThrowAsync(orderId, serviceId);
        
        var existingOrderPart = await GetOrderServicePartAsync(orderId, serviceId, request.PartId);

        if (existingOrderPart is not null)
        {
            existingOrderPart.Quantity += request.Quantity;
        }
        else
        {
            var part = await _entityValidationService.GetPartOrThrowAsync(request.PartId, true);
            
            var orderServicePart = new OrderServicePart
            {
                OrderId = orderId,
                ServiceId = serviceId,
                PartId = request.PartId,
                Quantity = request.Quantity,
                PriceAtMoment = part.DefaultPrice
            };

            _db.OrderServiceParts.Add(orderServicePart);
        }

        await _db.SaveChangesAsync();
    }

    private async Task<OrderServicePart?> GetOrderServicePartAsync(int orderId, int serviceId, int partId)
    {
        return await _db.OrderServiceParts
            .FirstOrDefaultAsync(osp =>
                osp.OrderId == orderId &&
                osp.ServiceId == serviceId &&
                osp.PartId == partId);
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
        await _entityValidationService.EnsureOrderExistsAsync(orderId);

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

        await using var transaction = await _db.Database.BeginTransactionAsync();

        order.StatusId = newStatusEntity.StatusId;
        
        await _db.SaveChangesAsync();

        await TryAddCommentToLastHistoryAsync(orderId, comment);

        await transaction.CommitAsync();
    }

    private async Task TryAddCommentToLastHistoryAsync(int orderId, string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return;

        var lastHistory = await _db.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .ThenByDescending(h => h.HistoryId)
            .FirstOrDefaultAsync();

        if (lastHistory is null)
            return;

        lastHistory.Comment = comment;
        await _db.SaveChangesAsync();
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

    public async Task UpdateOrderServicePartAsync(
        int orderId,
        int serviceId,
        int partId,
        EditOrderServicePartRequest request)
    {
        EnsureCurrentUserIsAdminOrManager();
        await _orderStatusService.EnsureOrderHasStatusToBeModifiedAsync(orderId);

        var orderPart = await _entityValidationService.GetOrderServicePartOrThrowAsync(orderId, serviceId, partId);

        orderPart.Quantity = request.Quantity;

        await _db.SaveChangesAsync();
    }

}