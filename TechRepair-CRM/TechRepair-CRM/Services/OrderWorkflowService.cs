using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services;

public class OrderWorkflowService
{
    private readonly RepairServiceDbContext _db;

    public OrderWorkflowService(RepairServiceDbContext db)
    {
        _db = db;
    }
    
    public async Task<ListResponse<OrderListItemResponse>> GetAllAsync()
    {
        var orders = _db.VwOrderFullInfos
            .OrderByDescending(o => o.CreatedAt);
        var count = await orders.CountAsync();
        var items = await orders
            .Select(order => new OrderListItemResponse(
                (int)order.OrderId,
                order.OrderNumber,
                (DateTime)order.CreatedAt,
                order.OrderStatus,
                order.ClientFirstName + " " + order.ClientLastName,
                order.ClientPhone,
                order.DeviceType,
                order.Brand,
                order.Model,
                (decimal)order.TotalCost,
                (decimal)order.PaidAmount,
                (decimal)order.RemainingAmount))
            .ToListAsync();
        
        return new ListResponse<OrderListItemResponse>(items, count);
    }
    
    public async Task<OrderDetailsResponse?> GetByIdAsync(int id)
    {
        var order =  await _db.VwOrderFullInfos
            .Where(o => o.OrderId == id)
            .Select(o => new OrderDetailsResponse(
                (int)o.OrderId,
                o.OrderNumber,
                (DateTime)o.CreatedAt,
                o.AcceptedAt,
                o.CompletedAt,
                o.IssuedAt,

                o.OrderStatus,

                (int)o.ClientId,
                o.ClientFirstName,
                o.ClientLastName,
                o.ClientPhone,
                o.ClientEmail,

                (int)o.DeviceId,
                o.DeviceType,
                o.Brand,
                o.Model,
                o.SerialNumber,
                o.EquipmentDescription,
                o.ExternalCondition,

                o.ProblemDescription,
                o.DiagnosticResult,
                (decimal)o.EstimatedCost,
                (decimal)o.TotalCost,
                (bool)o.IsWarrantyRepair,
                o.WarrantyMonths,
                o.OrderNotes,

                (decimal)o.ServiceSum,
                (decimal)o.PartSum,
                (decimal)o.PaidAmount,
                (decimal)o.RemainingAmount))
            .FirstOrDefaultAsync();

        return order;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var createdStatus = await _db.OrderStatuses
            .SingleAsync(s => s.StatusName == "Created");

        var orderId = await GetNextOrderIdAsync();

        var order = new RepairOrder
        {
            OrderId = orderId,
            OrderNumber = $"RO-{DateTime.Today:yyyyMMdd}-{orderId:D6}",
            DeviceId = request.DeviceId,
            StatusId = createdStatus.StatusId,
            ProblemDescription = request.ProblemDescription,
            EstimatedCost = request.EstimatedCost,
            WarrantyMonths = request.WarrantyMonths,
            IsWarrantyRepair = request.IsWarrantyRepair,
            DiagnosticResult = request.DiagnosticResult,
            Notes = request.Notes,
            CreatedAt = DateTime.Now,
            TotalCost = 0
        };

        _db.RepairOrders.Add(order);
        await _db.SaveChangesAsync();

        return new CreateOrderResponse(orderId, order.OrderNumber);
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