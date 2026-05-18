using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class AddServiceModel : PageModel
{
    private readonly ILookupService _lookupService;
    private readonly IOrderCommandService _orderCommandService;

    public AddServiceModel(ILookupService lookupService, IOrderCommandService orderCommandService)
    {
        _lookupService = lookupService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public AddOrderServiceRequest Input { get; set; } = new();

    public int OrderId { get; private set; }

    public IReadOnlyList<SelectListItem> Services { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Technicians { get; private set; } = [];

    public async Task OnGetAsync(int orderId)
    {
        OrderId = orderId;
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync(int orderId)
    {
        OrderId = orderId;

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            await _orderCommandService.AddServiceToOrderAsync(orderId, Input);
            return RedirectToPage("/Orders/AddServicePart", new { orderId, serviceId = Input.ServiceId!.Value });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadSelectListsAsync();
            return Page();
        }
    }

    private async Task LoadSelectListsAsync()
    {
        Services = await _lookupService.GetAvailableServicesForOrderAsync(OrderId);
        Technicians = await _lookupService.GetActiveTechniciansAsync();
    }
}