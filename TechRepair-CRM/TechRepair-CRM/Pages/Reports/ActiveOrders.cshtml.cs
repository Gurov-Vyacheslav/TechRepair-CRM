using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Reports;

namespace TechRepair_CRM.Pages.Reports;

[Authorize(Roles = "Admin,Manager")]
public class ActiveOrdersModel : PageModel
{
    private readonly IReportQueryService _reportQueryService;

    public ActiveOrdersModel(IReportQueryService reportQueryService)
    {
        _reportQueryService = reportQueryService;
    }

    public List<OrderListItemResponse> Orders { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Orders = await _reportQueryService.GetActiveOrdersAsync();
    }
}