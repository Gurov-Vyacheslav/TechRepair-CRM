using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Parts;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly IReferenceQueryService _referenceQueryService;

    public IndexModel(IReferenceQueryService referenceQueryService)
    {
        _referenceQueryService = referenceQueryService;
    }

    [BindProperty(SupportsGet = true)]
    public ReferenceFilterRequest Filter { get; set; } = new();

    public IReadOnlyList<PartItemResponse> Parts { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Parts = await _referenceQueryService.GetPartsAsync(Filter);
    }
}