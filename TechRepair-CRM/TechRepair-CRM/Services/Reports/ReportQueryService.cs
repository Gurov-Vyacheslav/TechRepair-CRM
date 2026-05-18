using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Reports;
using TechRepair_CRM.Models.Db;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Services.Reports;

public class ReportQueryService : IReportQueryService
{
    private readonly RepairServiceDbContext _db;

    public ReportQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OrderListItemResponse>> GetActiveOrdersAsync()
    {
        var orders = _db.VwOrderFullInfos
            .Where(o => o.OrderStatus != OrderStatus.Closed && o.OrderStatus != OrderStatus.Canceled);

        return await OrderListProjection.GetOrdersListItems(orders);
    }

    public async Task<IReadOnlyList<OrderListItemResponse>> GetUnpaidOrdersAsync()
    {
        var orders = _db.VwOrderFullInfos
            .Where(o =>
                o.RemainingAmount > 0 &&
                o.OrderStatus != OrderStatus.Canceled &&
                !(o.IsWarrantyRepair == true && o.OrderStatus == OrderStatus.Closed));

        return await OrderListProjection.GetOrdersListItems(orders);
    }

    public async Task<IReadOnlyList<TechnicianWorkloadReportItem>> GetTechnicianWorkloadAsync()
    {
        return await _db.VwTechnicianWorkloads
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new TechnicianWorkloadReportItem(
                t.TechnicianId!.Value,
                t.LastName + " " + t.FirstName,
                t.Specialization,
                t.IsActive ?? false,
                t.AssignedServiceRowsCount ?? 0,
                t.AssignedServiceQuantity ?? 0,
                t.CompletedServiceQuantity ?? 0,
                t.AssignedServiceAmount ?? 0,
                t.CompletedServiceAmount ?? 0,
                t.FirstCompletedAt,
                t.LastCompletedAt
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ServiceStatisticsReportItem>> GetServiceStatisticsAsync()
    {
        return await _db.VwServiceStatistics
            .OrderByDescending(s => s.TotalRevenue)
            .Select(s => new ServiceStatisticsReportItem(
                s.ServiceId!.Value,
                s.ServiceName!,
                s.IsActive!.Value,
                s.UsageCount!.Value,
                s.TotalQuantity!.Value,
                s.TotalRevenue!.Value,
                s.AvgPriceAtMoment
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PartUsageStatisticsReportItem>> GetPartUsageStatisticsAsync()
    {
        return await _db.VwPartUsageStatistics
            .OrderByDescending(p => p.TotalAmount)
            .Select(p => new PartUsageStatisticsReportItem(
                p.PartId!.Value,
                p.PartNumber,
                p.PartName!,
                p.Manufacturer,
                p.IsActive!.Value,
                p.UsageCount!.Value,
                p.TotalQuantity!.Value,
                p.TotalAmount!.Value
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RepairDurationReportItem>> GetRepairDurationAsync()
    {
        return await _db.VwRepairDurations
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RepairDurationReportItem(
                r.OrderId!.Value,
                r.OrderNumber!,
                r.CreatedAt!.Value,
                r.CompletedAt,
                r.OrderStatus!,
                r.RepairDurationHours,
                r.RepairDurationDays
            ))
            .ToListAsync();
    }
}