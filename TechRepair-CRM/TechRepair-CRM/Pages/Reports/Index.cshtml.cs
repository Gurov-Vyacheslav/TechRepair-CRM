using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TechRepair_CRM.Pages.Reports;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}