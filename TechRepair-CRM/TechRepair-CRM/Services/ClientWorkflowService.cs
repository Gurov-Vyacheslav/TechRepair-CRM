using TechRepair_CRM.Data;

namespace TechRepair_CRM.Services;

public class ClientWorkflowService
{
    private readonly RepairServiceDbContext _db;

    public ClientWorkflowService(RepairServiceDbContext db)
    {
        _db = db;
    }
}