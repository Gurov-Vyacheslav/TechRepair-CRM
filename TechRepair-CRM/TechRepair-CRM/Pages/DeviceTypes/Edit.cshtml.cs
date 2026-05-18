using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.DeviceTypes;
using TechRepair_CRM.Services.References;
namespace TechRepair_CRM.Pages.DeviceTypes;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly IReferenceQueryService _referenceQueryService; 
    private readonly IReferenceCommandService _referenceCommandService;

    public EditModel(
        IReferenceQueryService referenceQueryService,
        IReferenceCommandService referenceCommandService)
    {
        _referenceQueryService = referenceQueryService; 
        _referenceCommandService = referenceCommandService;
    }
    
    [BindProperty] public DeviceTypeFormRequest Input { get; set; } = new();
    public int DeviceTypeId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        DeviceTypeId = id; 
        var input = await _referenceQueryService.GetDeviceTypeFormAsync(id); 
        if (input is null) 
            return NotFound(); 
        Input = input; 
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        DeviceTypeId = id; 
        if (!ModelState.IsValid) 
            return Page();
        try
        {
            await _referenceCommandService.UpdateDeviceTypeAsync(id, Input);
            return RedirectToPage("/DeviceTypes/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message); 
            return Page();
        }
    }
}