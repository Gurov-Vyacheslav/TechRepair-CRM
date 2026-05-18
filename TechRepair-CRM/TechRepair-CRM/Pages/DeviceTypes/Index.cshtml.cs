using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.DeviceTypes;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.DeviceTypes;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IReferenceQueryService _referenceQueryService;
    private readonly IReferenceCommandService _referenceCommandService;
    
    public IndexModel(
        IReferenceQueryService referenceQueryService, 
        IReferenceCommandService referenceCommandService)
    {
        _referenceQueryService = referenceQueryService;
        _referenceCommandService = referenceCommandService;
    }
    public IReadOnlyList<DeviceTypeItemResponse> DeviceTypes { get; private set; } = [];
    
    public async Task OnGetAsync() 
        => DeviceTypes = await _referenceQueryService.GetDeviceTypesAsync();
    
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _referenceCommandService.DeleteDeviceTypeAsync(id);
            TempData["Success"] = "Тип устройства удалён.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }
}