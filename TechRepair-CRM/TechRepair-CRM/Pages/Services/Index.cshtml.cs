using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Services;

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

    public IReadOnlyList<ServiceItemResponse> Services { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Services = await _referenceQueryService.GetServicesAsync(Filter);
    }
}