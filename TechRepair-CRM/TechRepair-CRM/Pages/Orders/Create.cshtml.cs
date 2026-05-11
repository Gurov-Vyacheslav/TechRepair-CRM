using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class CreateModel : PageModel
{
    private readonly ILookupService _lookupService;
    private readonly IOrderCommandService _orderCommandService;

    public CreateModel(
        ILookupService lookupService,
        IOrderCommandService orderCommandService)
    {
        _lookupService = lookupService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public CreateOrderRequest Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> Devices { get; private set; } = [];

    public async Task OnGetAsync(int? deviceId)
    {
        if (deviceId is not null)
            Input.DeviceId = deviceId.Value;

        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            var orderId = await _orderCommandService.CreateOrderAsync(Input);
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
        Devices = await _lookupService.GetDevicesAsync();
    }
}