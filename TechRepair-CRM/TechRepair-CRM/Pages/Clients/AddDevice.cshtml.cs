using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.Services.Clients;
using TechRepair_CRM.Services.Lookups;

namespace TechRepair_CRM.Pages.Clients;

[Authorize(Roles = "Admin,Manager")]
public class AddDeviceModel : PageModel
{
    private readonly ILookupService _lookupService;
    private readonly IClientCommandService _clientCommandService;

    public AddDeviceModel(
        ILookupService lookupService,
        IClientCommandService clientCommandService)
    {
        _lookupService = lookupService;
        _clientCommandService = clientCommandService;
    }

    [BindProperty]
    public AddDeviceRequest Input { get; set; } = new();

    public int ClientId { get; private set; }

    public List<SelectListItem> DeviceTypes { get; private set; } = [];

    public async Task OnGetAsync(int clientId)
    {
        ClientId = clientId;
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync(int clientId)
    {
        ClientId = clientId;

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            var deviceId = await _clientCommandService.AddDeviceToClientAsync(clientId, Input);

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