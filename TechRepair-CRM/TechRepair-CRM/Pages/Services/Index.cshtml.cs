using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;

namespace TechRepair_CRM.Pages.Services;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly RepairServiceDbContext _db;

    public IndexModel(RepairServiceDbContext db)
    {
        _db = db;
    }

    public List<ServiceItem> Services { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Services = await _db.Services
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceItem(
                s.ServiceId,
                s.ServiceName,
                s.BasePrice,
                s.EstimatedDuration,
                s.IsActive
            ))
            .ToListAsync();
    }

    public record ServiceItem(
        int ServiceId,
        string ServiceName,
        decimal BasePrice,
        int? EstimatedDuration,
        bool IsActive
    );
}