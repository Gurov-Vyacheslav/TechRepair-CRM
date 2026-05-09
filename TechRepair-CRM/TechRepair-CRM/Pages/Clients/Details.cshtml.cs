using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.Services.Clients;

namespace TechRepair_CRM.Pages.Clients;

[Authorize(Roles = "Admin,Manager")]
public class DetailsModel : PageModel
{
    private readonly IClientQueryService _clientQueryService;

    public DetailsModel(IClientQueryService clientQueryService)
    {
        _clientQueryService = clientQueryService;
    }

    public ClientDetailsResponse Client { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = await _clientQueryService.GetClientDetailsAsync(id);

        if (client is null)
            return NotFound();

        Client = client;
        return Page();
    }
}