using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.DTOs.Orders.StatusHistory;

namespace TechRepair_CRM.Services.Orders;

public class OrderQueryService : IOrderQueryService
{
    private readonly RepairServiceDbContext _db;

    public OrderQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<OrderListItemResponse>> GetOrdersAsync()
    {
        var orders = await _db.VwOrderFullInfos
            .OrderByDescending(o => o.CreatedAt)
            .Select(order => new OrderListItemResponse(
                order.OrderId.Value,
                order.OrderNumber,
                order.CreatedAt.Value,
                order.OrderStatus,
                order.ClientFirstName + " " + order.ClientLastName,
                order.ClientPhone,
                order.DeviceType,
                order.Brand,
                order.Model,
                order.TotalCost ?? 0,
                order.PaidAmount ?? 0,
                order.RemainingAmount ?? 0))
            .ToListAsync();

        return orders;
    }
    
    public async Task<OrderDetailsResponse?> GetOrderDetailsAsync(int orderId)
    {
        var order = await GetOrderMainInfoAsync(orderId);

        if (order is null)
            return null;

        var services = await GetOrderServicesAsync(orderId);
        var parts = await GetOrderPartsAsync(orderId);
        var payments = await GetOrderPaymentsAsync(orderId);
        var history = await GetOrderHistoryAsync(orderId);
        
        return order with
        {
            Services = services,
            Parts = parts,
            Payments = payments,
            StatusHistory = history
        };
    }
    
    private async Task<OrderDetailsResponse?> GetOrderMainInfoAsync(int orderId)
    {
        return await _db.VwOrderFullInfos
            .Where(o => o.OrderId == orderId)
            .Select(o => new OrderDetailsResponse(
                o.OrderId.Value,
                o.OrderNumber,
                o.CreatedAt.Value,
                o.AcceptedAt,
                o.CompletedAt,
                o.IssuedAt,

                o.OrderStatus,

                o.ClientId.Value,
                o.ClientFirstName,
                o.ClientLastName,
                o.ClientPhone,
                o.ClientEmail,

                o.DeviceId.Value,
                o.DeviceType,
                o.Brand,
                o.Model,
                o.SerialNumber,
                o.EquipmentDescription,
                o.ExternalCondition,

                o.ProblemDescription,
                o.DiagnosticResult,
                o.EstimatedCost.Value,
                o.TotalCost.Value,
                o.IsWarrantyRepair.Value,
                o.WarrantyMonths,
                o.OrderNotes,

                o.ServiceSum ?? 0,
                o.PartSum ?? 0,
                o.PaidAmount ?? 0,
                o.RemainingAmount ?? 0,

                new List<OrderServiceResponse>(),
                new List<OrderPartResponse>(),
                new List<PaymentResponse>(),
                new List<OrderStatusHistoryResponse>()
            ))
            .FirstOrDefaultAsync();
    }

    private async Task<List<OrderServiceResponse>> GetOrderServicesAsync(int orderId)
    {
        return await _db.OrderServices
            .Where(os => os.OrderId == orderId)
            .Select(os => new OrderServiceResponse(
                os.OrderId,
                os.ServiceId,
                os.Service.ServiceName,
                os.TechnicianId,
                os.Technician == null
                    ? null
                    : os.Technician.FirstName + " " + os.Technician.LastName,
                os.Quantity,
                os.PriceAtMoment,
                os.Quantity * os.PriceAtMoment,
                os.CompletedAt,
                os.Notes
            ))
            .ToListAsync();
    }

    private async Task<List<OrderPartResponse>> GetOrderPartsAsync(int orderId)
    {
        return await _db.OrderParts
            .Where(op => op.OrderId == orderId)
            .Select(op => new OrderPartResponse(
                op.OrderId,
                op.PartId,
                op.Part.PartName,
                op.Part.PartNumber,
                op.Quantity,
                op.PriceAtMoment,
                op.Quantity * op.PriceAtMoment
            ))
            .ToListAsync();
    }

    private async Task<List<PaymentResponse>> GetOrderPaymentsAsync(int orderId)
    {
        return await _db.Payments
            .Where(p => p.OrderId == orderId)
            .OrderBy(p => p.PaymentDate)
            .Select(p => new PaymentResponse(
                p.PaymentId,
                p.OrderId,
                p.PaymentDate,
                p.Amount,
                p.PaymentMethod,
                p.TransactionNumber,
                p.Notes
            ))
            .ToListAsync();
    }

    private async Task<List<OrderStatusHistoryResponse>> GetOrderHistoryAsync(int orderId)
    {
        return await _db.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistoryResponse(
                h.HistoryId,
                h.OrderId,
                h.Status.StatusName,
                h.ChangedAt,
                h.Comment
            ))
            .ToListAsync();
    }
}