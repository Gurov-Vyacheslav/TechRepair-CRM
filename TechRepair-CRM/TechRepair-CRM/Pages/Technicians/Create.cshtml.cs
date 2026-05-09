using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.Technicians;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Technicians;

[Authorize(Roles = "Admin,Manager")]
public class CreateModel : PageModel
{
    private readonly IReferenceCommandService _referenceCommandService;

    public CreateModel(IReferenceCommandService referenceCommandService)
    {
        _referenceCommandService = referenceCommandService;
    }

    [BindProperty]
    public TechnicianFormRequest Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _referenceCommandService.CreateTechnicianAsync(Input);
            return RedirectToPage("/Technicians/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}