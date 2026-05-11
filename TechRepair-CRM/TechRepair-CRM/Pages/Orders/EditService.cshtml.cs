using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders.Services;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class EditServiceModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly IOrderCommandService _orderCommandService;
    private readonly ILookupService _lookupService;

    public EditServiceModel(
        IOrderQueryService orderQueryService,
        IOrderCommandService orderCommandService,
        ILookupService lookupService)
    {
        _orderQueryService = orderQueryService;
        _orderCommandService = orderCommandService;
        _lookupService = lookupService;
    }

    [BindProperty]
    public EditOrderServiceRequest Input { get; set; } = new();

    public int OrderId { get; private set; }
    public int ServiceId { get; private set; }

    public IReadOnlyList<SelectListItem> Technicians { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int orderId, int serviceId)
    {
        OrderId = orderId;
        ServiceId = serviceId;

        var input = await _orderQueryService.GetOrderServiceEditFormAsync(orderId, serviceId);

        if (input is null)
            return NotFound();

        Input = input;
        await LoadSelectListsAsync();

        return Page();
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
            await _orderCommandService.UpdateOrderServiceAsync(orderId, serviceId, Input);
            return RedirectToPage("/Orders/Details", new { id = orderId });
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
        Technicians = await _lookupService.GetActiveTechniciansAsync();
    }
}
