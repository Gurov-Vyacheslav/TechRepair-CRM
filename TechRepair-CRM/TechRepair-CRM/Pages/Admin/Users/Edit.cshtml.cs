using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Admin;
using TechRepair_CRM.Services.Admin;
using TechRepair_CRM.Services.Lookups;

namespace TechRepair_CRM.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly IUserAdminService _userAdminService;
    private readonly ILookupService _lookupService;

    public EditModel(IUserAdminService userAdminService, ILookupService lookupService)
    {
        _userAdminService = userAdminService;
        _lookupService = lookupService;
    }

    [BindProperty]
    public UserEditRequest Input { get; set; } = new();

    public List<SelectListItem> Roles { get; private set; } = [];
    public IReadOnlyList<TechnicianUserOptionResponse> Technicians { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userAdminService.GetUserForEditAsync(id);

        if (user is null)
            return NotFound();

        Input = user;
        await LoadSelectListsAsync(id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        Input.Id = id;

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync(id);
            return Page();
        }

        try
        {
            await _userAdminService.UpdateUserAsync(id, Input);
            return RedirectToPage("/Admin/Users/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadSelectListsAsync(id);
            return Page();
        }
    }

    private async Task LoadSelectListsAsync(string userId)
    {
        var roles = await _userAdminService.GetRolesAsync();

        Roles = roles
            .Select(role => new SelectListItem
            {
                Value = role,
                Text = role
            })
            .ToList();

        Technicians = await _userAdminService.GetAvailableTechnicianOptionsForEditAsync(userId);
    }
}
