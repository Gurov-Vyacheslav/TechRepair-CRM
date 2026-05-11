using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.Services.Clients;
using TechRepair_CRM.Services.Lookups;

namespace TechRepair_CRM.Pages.Clients;

[Authorize(Roles = "Admin,Manager")]
public class CreateWithDeviceModel : PageModel
{
    private readonly ILookupService _lookupService;
    private readonly IClientCommandService _clientCommandService;

    public CreateWithDeviceModel(
        ILookupService lookupService,
        IClientCommandService clientCommandService)
    {
        _lookupService = lookupService;
        _clientCommandService = clientCommandService;
    }

    [BindProperty]
    public CreateClientWithDeviceRequest Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> DeviceTypes { get; private set; } = [];

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
            var deviceId = await _clientCommandService.CreateClientWithDeviceAsync(Input);
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