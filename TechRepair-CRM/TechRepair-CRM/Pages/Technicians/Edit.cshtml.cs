using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.Technicians;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Technicians;

[Authorize(Roles = "Admin,Manager")]
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

    [BindProperty]
    public TechnicianFormRequest Input { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var technician = await _referenceQueryService.GetTechnicianFormAsync(id);

        if (technician is null)
            return NotFound();
        
        Input = technician;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _referenceCommandService.UpdateTechnicianAsync(id, Input);
            return RedirectToPage("/Technicians/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}