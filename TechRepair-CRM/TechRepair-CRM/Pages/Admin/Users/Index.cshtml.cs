using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Admin;
using TechRepair_CRM.Services.Admin;

namespace TechRepair_CRM.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IUserAdminService _userAdminService;

    public IndexModel(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [BindProperty(SupportsGet = true)]
    public UserFilterRequest Filter { get; set; } = new();

    public IReadOnlyList<UserListItemResponse> Users { get; private set; } = [];

    public IReadOnlyList<string> Roles { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Users = await _userAdminService.GetUsersAsync(Filter);
        Roles = await _userAdminService.GetRolesAsync();
    }
}