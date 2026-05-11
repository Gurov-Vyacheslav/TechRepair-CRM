using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Technicians;

namespace TechRepair_CRM.Pages.Technicians;

[Authorize(Roles = "Technician,Admin,Manager")]
public class MyOrdersModel : PageModel
{
    private readonly ITechnicianWorkService _technicianWorkService;

    public MyOrdersModel(ITechnicianWorkService technicianWorkService)
    {
        _technicianWorkService = technicianWorkService;
    }

    [BindProperty(SupportsGet = true)]
    public bool OnlyActive { get; set; } = true;

    public IReadOnlyList<OrderListItemResponse> Orders { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Orders = await _technicianWorkService.GetMyOrdersAsync(OnlyActive);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            Orders = [];
        }
    }
}