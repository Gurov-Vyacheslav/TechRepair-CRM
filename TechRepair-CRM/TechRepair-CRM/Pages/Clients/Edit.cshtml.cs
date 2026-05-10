using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.Services.Clients;

namespace TechRepair_CRM.Pages.Clients;

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly IClientQueryService _clientQueryService;
    private readonly IClientCommandService _clientCommandService;

    public EditModel(IClientQueryService clientQueryService, IClientCommandService clientCommandService)
    {
        _clientQueryService = clientQueryService;
        _clientCommandService = clientCommandService;
    }

    [BindProperty]
    public ClientFormRequest Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = await _clientQueryService.GetClientFormAsync(id);
        if (client is null)
            return NotFound();

        Input = client;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _clientCommandService.UpdateClientAsync(id, Input);
            return RedirectToPage("/Clients/Details", new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}