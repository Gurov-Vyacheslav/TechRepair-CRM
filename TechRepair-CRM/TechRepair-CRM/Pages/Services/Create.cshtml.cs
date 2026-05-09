using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Pages.Services;

[Authorize(Roles = "Admin,Manager")]
public class CreateModel : PageModel
{
    private readonly RepairServiceDbContext _db;

    public CreateModel(RepairServiceDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ServiceFormRequest Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var service = new Service
        {
            ServiceName = Input.ServiceName,
            Description = Input.Description,
            BasePrice = Input.BasePrice,
            EstimatedDuration = Input.EstimatedDuration,
            IsActive = Input.IsActive
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Services/Index");
    }
}