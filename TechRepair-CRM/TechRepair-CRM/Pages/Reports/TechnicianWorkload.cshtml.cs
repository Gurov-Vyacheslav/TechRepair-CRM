using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Reports;
using TechRepair_CRM.Services.Reports;

namespace TechRepair_CRM.Pages.Reports;

[Authorize(Roles = "Admin,Manager")]
public class TechnicianWorkloadModel : PageModel
{
    private readonly IReportQueryService _reportQueryService;

    public TechnicianWorkloadModel(IReportQueryService reportQueryService)
    {
        _reportQueryService = reportQueryService;
    }

    public IReadOnlyList<TechnicianWorkloadReportItem> Items { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Items = await _reportQueryService.GetTechnicianWorkloadAsync();
    }
}