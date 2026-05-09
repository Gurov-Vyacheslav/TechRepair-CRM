using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.References.Services;

namespace TechRepair_CRM.Pages.Services;

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly RepairServiceDbContext _db;

    public EditModel(RepairServiceDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ServiceFormRequest Input { get; set; } = new();

    public int ServiceId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var service = await _db.Services.FindAsync(id);

        if (service is null)
            return NotFound();

        ServiceId = id;

        Input = new ServiceFormRequest
        {
            ServiceName = service.ServiceName,
            Description = service.Description,
            BasePrice = service.BasePrice,
            EstimatedDuration = service.EstimatedDuration,
            IsActive = service.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        ServiceId = id;

        if (!ModelState.IsValid)
            return Page();

        var service = await _db.Services.FindAsync(id);

        if (service is null)
            return NotFound();

        service.ServiceName = Input.ServiceName;
        service.Description = Input.Description;
        service.BasePrice = Input.BasePrice;
        service.EstimatedDuration = Input.EstimatedDuration;
        service.IsActive = Input.IsActive;

        await _db.SaveChangesAsync();

        return RedirectToPage("/Services/Index");
    }
}