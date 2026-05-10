using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Services;

[Authorize(Roles = "Admin,Manager")]
public class CreateModel : PageModel
{
    private readonly IReferenceCommandService _referenceCommandService;

    public CreateModel(IReferenceCommandService referenceCommandService)
    {
        _referenceCommandService = referenceCommandService;
    }

    [BindProperty]
    public ServiceFormRequest Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _referenceCommandService.CreateServiceAsync(Input);
            return RedirectToPage("/Services/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}