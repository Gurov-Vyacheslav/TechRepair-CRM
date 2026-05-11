using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Technicians;
using TechRepair_CRM.Services.Technicians;

namespace TechRepair_CRM.Pages.Technicians;

[Authorize(Roles = "Technician,Admin,Manager")]
public class MyWorkModel : PageModel
{
    private readonly ITechnicianWorkService _technicianWorkService;

    public MyWorkModel(ITechnicianWorkService technicianWorkService)
    {
        _technicianWorkService = technicianWorkService;
    }

    [BindProperty(SupportsGet = true)]
    public bool OnlyActive { get; set; } = true;

    public IReadOnlyList<MyWorkItemResponse> Items { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Items = await _technicianWorkService.GetMyWorkAsync(OnlyActive);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            Items = [];
        }
    }

    public async Task<IActionResult> OnPostCompleteAsync(int orderId, int serviceId)
    {
        try
        {
            await _technicianWorkService.CompleteMyServiceAsync(orderId, serviceId);
            TempData["Success"] = "Услуга отмечена как выполненная.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage(new { OnlyActive });
    }
}