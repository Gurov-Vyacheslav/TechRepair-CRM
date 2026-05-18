using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Orders;

public static class OrderListProjection
{
    public static async Task<IReadOnlyList<OrderListItemResponse>> GetOrdersListItems(IQueryable<VwOrderFullInfo> orders)
    {
        return await orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItemResponse(
                o.OrderId!.Value,
                o.OrderNumber!,
                o.CreatedAt!.Value,
                o.OrderStatus!,
                o.ClientFirstName + " " + o.ClientLastName,
                o.ClientPhone!,
                o.DeviceType!,
                o.Brand,
                o.Model,
                o.IsWarrantyRepair ?? false,
                o.TotalCost ?? 0,
                o.PaidAmount ?? 0,
                o.RemainingAmount ?? 0
            ))
            .ToListAsync();
    }
}