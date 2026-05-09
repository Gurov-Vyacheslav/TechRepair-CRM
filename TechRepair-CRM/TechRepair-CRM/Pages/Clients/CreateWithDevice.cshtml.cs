using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Clients;

[Authorize(Roles = "Admin,Manager")]
public class CreateWithDeviceModel : PageModel
{
    private readonly ILookupService _lookupService;
    private readonly IOrderCommandService _orderCommandService;

    public CreateWithDeviceModel(
        ILookupService lookupService,
        IOrderCommandService orderCommandService)
    {
        _lookupService = lookupService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public CreateClientWithDeviceRequest Input { get; set; } = new();

    public List<SelectListItem> DeviceTypes { get; private set; } = [];

    public async Task OnGetAsync()
    {
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
            var deviceId = await _orderCommandService.CreateClientWithDeviceAsync(Input);
            return RedirectToPage("/Orders/Create", new { deviceId });
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
        DeviceTypes = await _lookupService.GetDeviceTypesAsync();
    }
}