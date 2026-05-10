using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.Services.Devices;
using TechRepair_CRM.Services.Lookups;

namespace TechRepair_CRM.Pages.Devices;

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly IDeviceQueryService _deviceQueryService;
    private readonly IDeviceCommandService _deviceCommandService;
    private readonly ILookupService _lookupService;

    public EditModel(IDeviceQueryService deviceQueryService, IDeviceCommandService deviceCommandService, ILookupService lookupService)
    {
        _deviceQueryService = deviceQueryService;
        _deviceCommandService = deviceCommandService;
        _lookupService = lookupService;
    }

    [BindProperty]
    public DeviceFormRequest Input { get; set; } = new();

    public List<SelectListItem> DeviceTypes { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var device = await _deviceQueryService.GetDeviceFormAsync(id);
        if (device is null)
            return NotFound();

        Input = device;
        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            var clientId = await _deviceCommandService.UpdateDeviceAsync(id, Input);
            return RedirectToPage("/Clients/Details", new { id = clientId });
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