using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Devices;

namespace TechRepair_CRM.Services.Devices;

public class DeviceQueryService : IDeviceQueryService
{
    private readonly RepairServiceDbContext _db;

    public DeviceQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<DeviceFormRequest?> GetDeviceFormAsync(int deviceId)
    {
        return await _db.Devices
            .Where(d => d.DeviceId == deviceId)
            .Select(d => new DeviceFormRequest
            {
                DeviceTypeId = d.DeviceTypeId,
                Brand = d.Brand,
                Model = d.Model,
                SerialNumber = d.SerialNumber,
                PurchaseDate = d.PurchaseDate,
                EquipmentDescription = d.EquipmentDescription,
                ExternalCondition = d.ExternalCondition,
                Notes = d.Notes
            })
            .FirstOrDefaultAsync();
    }
}