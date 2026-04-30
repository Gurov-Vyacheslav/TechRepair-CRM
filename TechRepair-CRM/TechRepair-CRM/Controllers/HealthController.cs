using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;

namespace TechRepair_CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly RepairServiceDbContext _db;

    public HealthController(RepairServiceDbContext db)
    {
        _db = db;
    }

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        var statusesCount = await _db.OrderStatuses.CountAsync();

        return Ok(new
        {
            database = "ok",
            statusesCount
        });
    }
}