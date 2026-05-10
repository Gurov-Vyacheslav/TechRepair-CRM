using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;

namespace TechRepair_CRM.Services.Lookups;

public class LookupService : ILookupService
{
    private readonly RepairServiceDbContext _db;

    public LookupService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<List<SelectListItem>> GetDeviceTypesAsync()
    {
        return await _db.DeviceTypes
            .OrderBy(t => t.TypeName)
            .Select(t => new SelectListItem
            {
                Value = t.DeviceTypeId.ToString(),
                Text = t.TypeName
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetDevicesAsync()
    {
        return await (
            from d in _db.Devices
            join c in _db.Clients on d.ClientId equals c.ClientId
            join dt in _db.DeviceTypes on d.DeviceTypeId equals dt.DeviceTypeId
            orderby c.LastName, c.FirstName
            select new SelectListItem
            {
                Value = d.DeviceId.ToString(),
                Text = c.LastName + " " + c.FirstName + " — " +
                       dt.TypeName + " " +
                       (d.Brand ?? "") + " " +
                       (d.Model ?? "") + " " +
                       (d.SerialNumber ?? "")
            }
        ).ToListAsync();
    }

    public async Task<List<SelectListItem>> GetClientsAsync()
    {
        return await _db.Clients
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new SelectListItem
            {
                Value = c.ClientId.ToString(),
                Text = c.LastName + " " + c.FirstName + " — " + c.Phone
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetOrderStatusesAsync()
    {
        return await _db.OrderStatuses
            .OrderBy(s => s.StatusId)
            .Select(s => new SelectListItem
            {
                Value = s.StatusName,
                Text = s.StatusName
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetActiveServicesAsync()
    {
        return await _db.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.ServiceName + " — " + s.BasePrice + " ₽"
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetActiveTechniciansAsync()
    {
        return await _db.Technicians
            .Where(t => t.IsActive)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new SelectListItem
            {
                Value = t.TechnicianId.ToString(),
                Text = t.LastName + " " + t.FirstName
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetActivePartsAsync()
    {
        return await _db.Parts
            .Where(p => p.IsActive)
            .OrderBy(p => p.PartName)
            .Select(p => new SelectListItem
            {
                Value = p.PartId.ToString(),
                Text = p.PartName + " — " + (p.DefaultPrice ?? 0) + " ₽"
            })
            .ToListAsync();
    }
}
