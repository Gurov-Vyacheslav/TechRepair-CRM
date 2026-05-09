using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Reports;

namespace TechRepair_CRM.Services.Reports;

public class ReportQueryService : IReportQueryService
{
    private readonly RepairServiceDbContext _db;

    public ReportQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<List<OrderListItemResponse>> GetActiveOrdersAsync()
    {
        return await _db.VwOrderFullInfos
            .Where(o => o.OrderStatus != "Closed" && o.OrderStatus != "Canceled")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItemResponse(
                o.OrderId.Value,
                o.OrderNumber,
                o.CreatedAt.Value,
                o.OrderStatus,
                o.ClientFirstName + " " + o.ClientLastName,
                o.ClientPhone,
                o.DeviceType,
                o.Brand,
                o.Model,
                o.TotalCost.Value,
                o.PaidAmount.Value,
                o.RemainingAmount.Value
            ))
            .ToListAsync();
    }

    public async Task<List<OrderListItemResponse>> GetUnpaidOrdersAsync()
    {
        return await _db.VwOrderFullInfos
            .Where(o => o.RemainingAmount > 0 && o.OrderStatus != "Canceled")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItemResponse(
                o.OrderId.Value,
                o.OrderNumber,
                o.CreatedAt.Value,
                o.OrderStatus,
                o.ClientFirstName + " " + o.ClientLastName,
                o.ClientPhone,
                o.DeviceType,
                o.Brand,
                o.Model,
                o.TotalCost.Value,
                o.PaidAmount.Value,
                o.RemainingAmount.Value
            ))
            .ToListAsync();
    }

    public async Task<List<TechnicianWorkloadReportItem>> GetTechnicianWorkloadAsync()
    {
        return await _db.VwTechnicianWorkloads
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new TechnicianWorkloadReportItem(
                t.TechnicianId.Value,
                t.FirstName,
                t.LastName,
                t.Specialization,
                t.IsActive.Value,
                t.AssignedServicesCount.Value,
                t.CompletedServicesCount.Value,
                t.TotalServiceQuantity.Value,
                t.TotalServiceAmount.Value
            ))
            .ToListAsync();
    }

    public async Task<List<ServiceStatisticsReportItem>> GetServiceStatisticsAsync()
    {
        return await _db.VwServiceStatistics
            .OrderByDescending(s => s.TotalRevenue)
            .Select(s => new ServiceStatisticsReportItem(
                s.ServiceId.Value,
                s.ServiceName,
                s.IsActive.Value,
                s.UsageCount.Value,
                s.TotalQuantity.Value,
                s.TotalRevenue.Value,
                s.AvgPriceAtMoment
            ))
            .ToListAsync();
    }

    public async Task<List<PartUsageStatisticsReportItem>> GetPartUsageStatisticsAsync()
    {
        return await _db.VwPartUsageStatistics
            .OrderByDescending(p => p.TotalAmount)
            .Select(p => new PartUsageStatisticsReportItem(
                p.PartId.Value,
                p.PartNumber,
                p.PartName,
                p.Manufacturer,
                p.IsActive.Value,
                p.UsageCount.Value,
                p.TotalQuantity.Value,
                p.TotalAmount.Value
            ))
            .ToListAsync();
    }

    public async Task<List<RepairDurationReportItem>> GetRepairDurationAsync()
    {
        return await _db.VwRepairDurations
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RepairDurationReportItem(
                r.OrderId.Value,
                r.OrderNumber,
                r.CreatedAt.Value,
                r.CompletedAt,
                r.OrderStatus,
                r.RepairDurationHours,
                r.RepairDurationDays
            ))
            .ToListAsync();
    }
}