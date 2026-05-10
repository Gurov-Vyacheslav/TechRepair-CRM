using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Services;

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly IReferenceQueryService _referenceQueryService;
    private readonly IReferenceCommandService _referenceCommandService;

    public EditModel(IReferenceQueryService referenceQueryService, IReferenceCommandService referenceCommandService)
    {
        _referenceQueryService = referenceQueryService;
        _referenceCommandService = referenceCommandService;
    }

    [BindProperty]
    public ServiceFormRequest Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var service = await _referenceQueryService.GetServiceFormAsync(id);
        if (service is null) return NotFound();
        Input = service;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _referenceCommandService.UpdateServiceAsync(id, Input);
            return RedirectToPage("/Services/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}