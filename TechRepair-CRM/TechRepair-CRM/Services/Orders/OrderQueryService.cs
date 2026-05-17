using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.DTOs.Orders.Services.Parts;
using TechRepair_CRM.DTOs.Orders.StatusHistory;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Orders;

public class OrderQueryService : IOrderQueryService
{
    private readonly RepairServiceDbContext _db;

    public OrderQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    
    public async Task<IReadOnlyList<OrderListItemResponse>> GetOrdersAsync(OrderFilterRequest? filter = null)
    {
        var query = GetFilteredOrders(filter);

        return await OrderListProjection.GetOrdersListItems(query);
    }
    

    private IQueryable<VwOrderFullInfo> GetFilteredOrders(OrderFilterRequest? filter)
    {
        var orders = _db.VwOrderFullInfos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter?.OrderNumber))
        {
            orders = orders.Where(o => o.OrderNumber!.Contains(filter.OrderNumber));
        }

        if (!string.IsNullOrWhiteSpace(filter?.Status))
        {
            orders = orders.Where(o => o.OrderStatus == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter?.ClientPhone))
        {
            orders = orders.Where(o => o.ClientPhone!.Contains(filter.ClientPhone));
        }

        if (filter?.CreatedFrom is not null)
        {
            orders = orders.Where(o => o.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter?.CreatedTo is not null)
        {
            var toExclusive = filter.CreatedTo.Value.Date.AddDays(1);
            orders = orders.Where(o => o.CreatedAt < toExclusive);
        }
        
        if (filter?.TechnicianId is not null)
        {
            orders = orders.Where(o =>
                _db.OrderServices.Any(os =>
                    os.OrderId == o.OrderId &&
                    os.TechnicianId == filter.TechnicianId.Value));
        }

        if (filter?.DeviceTypeId is not null)
        {
            orders = orders.Where(o =>
                _db.Devices.Any(d =>
                    d.DeviceId == o.DeviceId &&
                    d.DeviceTypeId == filter.DeviceTypeId.Value));
        }

        if (filter?.HasDebt is not null)
        {
            orders = filter.HasDebt.Value
                ? orders.Where(o => o.RemainingAmount > 0)
                : orders.Where(o => o.RemainingAmount <= 0);
        }
        
        return orders;
    } 
    
    public async Task<OrderEditRequest?> GetOrderEditFormAsync(int orderId)
    {
        return await _db.RepairOrders
            .Where(o => o.OrderId == orderId)
            .Select(o => new OrderEditRequest
            {
                ProblemDescription = o.ProblemDescription,
                DiagnosticResult = o.DiagnosticResult,
                EstimatedCost = o.EstimatedCost,
                WarrantyMonths = o.WarrantyMonths,
                IsWarrantyRepair = o.IsWarrantyRepair,
                Notes = o.Notes
            })
            .FirstOrDefaultAsync();
    }
    
    public async Task<OrderDetailsResponse?> GetOrderDetailsAsync(int orderId)
    {
        var order = await GetOrderMainInfoAsync(orderId);

        if (order is null)
            return null;

        var services = await GetOrderServicesAsync(orderId);
        var serviceParts = await GetOrderServicePartsAsync(orderId);
        var payments = await GetOrderPaymentsAsync(orderId);
        var history = await GetOrderHistoryAsync(orderId);

        var partsByService = serviceParts
            .GroupBy(p => p.ServiceId)
            .ToDictionary(g => g.Key,
                IReadOnlyList<OrderServicePartResponse> (g) => g.ToList());

        var servicesWithParts = services
            .Select(s => s with
            {
                Parts = partsByService.GetValueOrDefault(s.ServiceId)
                        ?? new List<OrderServicePartResponse>()
            })
            .ToList();

        return order with
        {
            Services = servicesWithParts,
            Payments = payments,
            StatusHistory = history
        };
    }
    
    private async Task<OrderDetailsResponse?> GetOrderMainInfoAsync(int orderId)
    {
        return await _db.VwOrderFullInfos
            .Where(o => o.OrderId == orderId)
            .Select(o => new OrderDetailsResponse(
                o.OrderId!.Value,
                o.OrderNumber!,
                o.CreatedAt!.Value,
                o.AcceptedAt,
                o.CompletedAt,
                o.IssuedAt,

                o.OrderStatus!,

                o.ClientId!.Value,
                o.ClientFirstName!,
                o.ClientLastName!,
                o.ClientPhone!,
                o.ClientEmail,

                o.DeviceId!.Value,
                o.DeviceType!,
                o.Brand,
                o.Model,
                o.SerialNumber,
                o.EquipmentDescription,
                o.ExternalCondition,

                o.ProblemDescription!,
                o.DiagnosticResult,
                o.EstimatedCost!.Value,
                o.TotalCost!.Value,
                o.IsWarrantyRepair!.Value,
                o.WarrantyMonths,
                o.OrderNotes,

                o.ServiceSum ?? 0,
                o.PartSum ?? 0,
                o.PaidAmount ?? 0,
                o.RemainingAmount ?? 0,

                new List<OrderServiceResponse>(),
                new List<PaymentResponse>(),
                new List<OrderStatusHistoryResponse>()
            ))
            .FirstOrDefaultAsync();
    }

    private async Task<IReadOnlyList<OrderServiceResponse>> GetOrderServicesAsync(int orderId)
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
                os.Notes,
                new List<OrderServicePartResponse>()
            ))
            .ToListAsync();
    }

    private async Task<IReadOnlyList<OrderServicePartResponse>> GetOrderServicePartsAsync(int orderId)
    {
        return await _db.OrderServiceParts
            .Where(osp => osp.OrderId == orderId)
            .Select(osp => new OrderServicePartResponse(
                osp.OrderId,
                osp.ServiceId,
                osp.PartId,
                osp.Part.PartName,
                osp.Part.PartNumber,
                osp.Quantity,
                osp.PriceAtMoment,
                osp.Quantity * osp.PriceAtMoment
            ))
            .ToListAsync();
    }

    private async Task<IReadOnlyList<PaymentResponse>> GetOrderPaymentsAsync(int orderId)
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

    private async Task<IReadOnlyList<OrderStatusHistoryResponse>> GetOrderHistoryAsync(int orderId)
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
    
    public async Task<EditOrderServiceRequest?> GetOrderServiceEditFormAsync(int orderId, int serviceId)
    {
        return await _db.OrderServices
            .Where(os => os.OrderId == orderId && os.ServiceId == serviceId)
            .Select(os => new EditOrderServiceRequest
            {
                Quantity = os.Quantity,
                TechnicianId = os.TechnicianId,
                Notes = os.Notes
            })
            .FirstOrDefaultAsync();
    }

    public async Task<EditOrderServicePartRequest?> GetOrderServicePartEditFormAsync(int orderId, int serviceId, int partId)
    {
        return await _db.OrderServiceParts
            .Where(osp => 
                osp.OrderId == orderId 
                && osp.ServiceId == serviceId 
                && osp.PartId == partId)
            .Select(osp => new EditOrderServicePartRequest
            {
                Quantity = osp.Quantity
            })
            .FirstOrDefaultAsync();
    }
}