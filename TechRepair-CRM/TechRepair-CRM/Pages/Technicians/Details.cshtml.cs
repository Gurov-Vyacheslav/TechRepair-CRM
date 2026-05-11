using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.DTOs.Technicians;
using TechRepair_CRM.Services.Technicians;

namespace TechRepair_CRM.Pages.Technicians;

[Authorize(Roles = "Admin,Manager")]
public class DetailsModel : PageModel
{
    private readonly ITechnicianWorkService _technicianWorkService;

    public DetailsModel(ITechnicianWorkService technicianWorkService)
    {
        _technicianWorkService = technicianWorkService;
    }

    [BindProperty(SupportsGet = true)]
    public string Tab { get; set; } = "work";

    public TechnicianProfileResponse Technician { get; private set; } = null!;

    public IReadOnlyList<MyWorkItemResponse> Works { get; private set; } = [];

    public IReadOnlyList<OrderListItemResponse> Orders { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var technician = await _technicianWorkService.GetTechnicianProfileAsync(id);

        if (technician is null)
            return NotFound();

        Technician = technician;

        if (Tab == "orders")
        {
            Orders = await _technicianWorkService.GetTechnicianOrdersAsync(id);
        }
        else
        {
            Works = await _technicianWorkService.GetTechnicianWorkAsync(id);
            Tab = "work";
        }

        return Page();
    }
}