using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders.Services.Parts;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class AddServicePartModel : PageModel
{
    private readonly IOrderCommandService _orderCommandService;
    private readonly ILookupService _lookupService;

    public AddServicePartModel(
        IOrderCommandService orderCommandService,
        ILookupService lookupService)
    {
        _orderCommandService = orderCommandService;
        _lookupService = lookupService;
    }

    [BindProperty]
    public AddOrderServicePartRequest Input { get; set; } = new();

    public int OrderId { get; private set; }
    public int ServiceId { get; private set; }

    public IReadOnlyList<SelectListItem> Parts { get; private set; } = [];

    public async Task OnGetAsync(int orderId, int serviceId)
    {
        OrderId = orderId;
        ServiceId = serviceId;

        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync(int orderId, int serviceId)
    {
        OrderId = orderId;
        ServiceId = serviceId;

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            await _orderCommandService.AddPartToOrderServiceAsync(orderId, serviceId, Input);
            return RedirectToPage("/Orders/AddServicePart", new { orderId, serviceId });
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
        Parts = await _lookupService.GetActivePartsAsync();
    }
}