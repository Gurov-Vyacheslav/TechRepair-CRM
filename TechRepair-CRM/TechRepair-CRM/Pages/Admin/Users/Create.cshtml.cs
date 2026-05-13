using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Admin;
using TechRepair_CRM.Services.Admin;
using TechRepair_CRM.Services.Lookups;

namespace TechRepair_CRM.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly IUserAdminService _userAdminService;
    private readonly ILookupService _lookupService;

    public CreateModel(IUserAdminService userAdminService, ILookupService lookupService)
    {
        _userAdminService = userAdminService;
        _lookupService = lookupService;
    }

    [BindProperty]
    public UserCreateRequest Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> Roles { get; private set; } = [];
    public IReadOnlyList<TechnicianUserOptionResponse> Technicians { get; private set; } = [];

    public async Task OnGetAsync(int? technicianId)
    {
        await LoadSelectListsAsync();

        if (technicianId is not null)
        {
            Input.Role = "Technician";
            Input.TechnicianId = technicianId.Value;

            var technicianEmail = await _userAdminService.GetTechnicianEmailAsync(technicianId.Value);

            if (!string.IsNullOrWhiteSpace(technicianEmail))
                Input.Email = technicianEmail;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            await _userAdminService.CreateUserAsync(Input);
            return RedirectToPage("/Admin/Users/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadSelectListsAsync();
            return Page();
        }
    }

    private async Task LoadSelectListsAsync()
    {
        var roles = await _userAdminService.GetRolesAsync();

        Roles = roles
            .Select(role => new SelectListItem
            {
                Value = role,
                Text = role
            })
            .ToList();

        Technicians = await _userAdminService.GetAvailableTechnicianOptionsForCreateAsync();
    }
}