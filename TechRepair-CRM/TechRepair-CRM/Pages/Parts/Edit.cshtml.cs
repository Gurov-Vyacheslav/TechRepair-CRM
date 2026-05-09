using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Parts;

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
    public PartFormRequest Input { get; set; } = new();

    public int PartId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var part = await _referenceQueryService.GetPartFormAsync(id);

        if (part is null)
            return NotFound();

        PartId = id;
        Input = part;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        PartId = id;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _referenceCommandService.UpdatePartAsync(id, Input);
            return RedirectToPage("/Parts/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}