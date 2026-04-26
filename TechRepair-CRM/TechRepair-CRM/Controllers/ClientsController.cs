using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly RepairServiceDbContext _context;

    public ClientsController(RepairServiceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Client>>> GetAll()
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Client>> GetById(int id)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClientId == id);

        if (client is null)
            return NotFound();

        return Ok(client);
    }
}