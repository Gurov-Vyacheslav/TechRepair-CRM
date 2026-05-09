using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Reports;

namespace TechRepair_CRM.Services.Reports;

public interface IReportQueryService
{
    Task<List<OrderListItemResponse>> GetActiveOrdersAsync();

    Task<List<OrderListItemResponse>> GetUnpaidOrdersAsync();

    Task<List<TechnicianWorkloadReportItem>> GetTechnicianWorkloadAsync();

    Task<List<ServiceStatisticsReportItem>> GetServiceStatisticsAsync();

    Task<List<PartUsageStatisticsReportItem>> GetPartUsageStatisticsAsync();

    Task<List<RepairDurationReportItem>> GetRepairDurationAsync();
}