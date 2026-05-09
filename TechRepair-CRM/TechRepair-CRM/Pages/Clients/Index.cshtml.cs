using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.Services.Clients;

namespace TechRepair_CRM.Pages.Clients;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly IClientQueryService _clientQueryService;

    public IndexModel(IClientQueryService clientQueryService)
    {
        _clientQueryService = clientQueryService;
    }

    public List<ClientListItemResponse> Clients { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Clients = await _clientQueryService.GetClientsAsync();
    }
}