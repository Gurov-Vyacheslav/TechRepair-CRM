using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Devices;

public class DeviceCommandService : IDeviceCommandService
{
    private readonly RepairServiceDbContext _db;

    public DeviceCommandService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<int> UpdateDeviceAsync(int deviceId, DeviceFormRequest request)
    {
        var device = await CheckDeviceExists(deviceId);
        
        device.DeviceTypeId = request.DeviceTypeId;
        device.Brand = request.Brand;
        device.Model = request.Model;
        device.SerialNumber = request.SerialNumber;
        device.PurchaseDate = request.PurchaseDate;
        device.EquipmentDescription = request.EquipmentDescription;
        device.ExternalCondition = request.ExternalCondition;
        device.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return device.ClientId;
    }

    private async Task<Device> CheckDeviceExists(int deviceId)
    {
        var device = await _db.Devices.FindAsync(deviceId);

        return device ?? throw new InvalidOperationException("Устройство не найдено.");
    }
}