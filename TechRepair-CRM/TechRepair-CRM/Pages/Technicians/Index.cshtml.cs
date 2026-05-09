using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.References.Technicians;
using TechRepair_CRM.Services.References;

namespace TechRepair_CRM.Pages.Technicians;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly IReferenceQueryService _referenceQueryService;

    public IndexModel(IReferenceQueryService referenceQueryService)
    {
        _referenceQueryService = referenceQueryService;
    }

    public List<TechnicianItemResponse> Technicians { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Technicians = await _referenceQueryService.GetTechniciansAsync();
    }
}