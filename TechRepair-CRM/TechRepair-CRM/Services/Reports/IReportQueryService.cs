using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Reports;

namespace TechRepair_CRM.Services.Reports;

public interface IReportQueryService
{
    Task<IReadOnlyList<OrderListItemResponse>> GetActiveOrdersAsync();

    Task<IReadOnlyList<OrderListItemResponse>> GetUnpaidOrdersAsync();

    Task<IReadOnlyList<TechnicianWorkloadReportItem>> GetTechnicianWorkloadAsync();

    Task<IReadOnlyList<ServiceStatisticsReportItem>> GetServiceStatisticsAsync();

    Task<IReadOnlyList<PartUsageStatisticsReportItem>> GetPartUsageStatisticsAsync();

    Task<IReadOnlyList<RepairDurationReportItem>> GetRepairDurationAsync();
}